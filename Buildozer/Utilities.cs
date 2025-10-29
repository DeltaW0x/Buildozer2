using System.Diagnostics;
namespace Buildozer;

public static class Utils
{
    public struct CommandReturn
    {
        public string Stdout;
        public string Stderr;
        public int ExitCode;
    }
    
    public static Version ParseClangVersionStdout(string stdout)
    {
        string[][] lines = stdout
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        return new Version(int.Parse(lines[0][^1]), int.Parse(lines[1][^1]), int.Parse(lines[2][^1]));
    }
    
    public static CommandReturn RunCommand(string shell, string command)
    {
        var process = new Process();
        process.StartInfo.FileName = shell;
        process.StartInfo.Arguments = OperatingSystem.IsWindows() ? $"{command}" : $"""-c "{command}" """;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        int exitCode = process.ExitCode;
        
        return new CommandReturn{Stdout = output, Stderr = error, ExitCode = exitCode};
    }

    private static async Task DownloadFileAsync(string url, string targetDir)
    {
        using (var client = new HttpClient())
        {
            var downloadStream = await client.GetStreamAsync(new Uri(url));
            var downloadFile = new FileStream(targetDir, FileMode.Create, FileAccess.Write);

            await downloadStream.CopyToAsync(downloadFile);
            await downloadFile.FlushAsync();
            downloadFile.Close();
        }
    }

    private static void RunGitClone(string repoUrl, string targetDir, bool recursive = true, string repoName = "repo")
    {
        string arguments = recursive ? "clone --progress --recursive" : "clone --progress";
        using var process = new Process
        {
            StartInfo =
                new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"{arguments} {repoUrl} {targetDir}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
        };
        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += new DataReceivedEventHandler((sender, args) => { Console.WriteLine(args.Data); });

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }
}