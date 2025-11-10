using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Buildozer;

public static class Utils
{
    public static string HashPath(string path)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(path)));
    }

    public struct CommandReturn
    {
        public string Stdout;
        public string Stderr;
        public int ExitCode;
    }

    public static Version ParseClangVersionStdout(string stdout)
    {
        var lines = stdout.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
        var regex = new Regex("__clang_(major|minor|patchlevel)__");
        
        string[][] versionLines = lines
            .Where(l => regex.IsMatch(l)).OrderBy(l => l)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        return new Version(int.Parse(versionLines[0][^1]), int.Parse(versionLines[1][^1]), int.Parse(versionLines[2][^1]));
    }
    
    public static CommandReturn RunProcess(string exe, string arguments)
    {
        Process process = new();
        process.StartInfo.FileName = exe;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new CommandReturn
        {
            Stdout = process.StandardOutput.ReadToEnd(), 
            Stderr = process.StandardError.ReadToEnd(), 
            ExitCode = process.ExitCode
        };
    }

    public static bool CheckProgram(string programName, out string? path)
    {
        CommandReturn ret;
        if (OperatingSystem.IsWindows())
            ret = RunProcess("cmd.exe", $"/c \"where {programName}\"");
        else
            ret = RunProcess("sh", $"-c \"which {programName}\"");
        
        path = ret.ExitCode == 0 ? ret.Stdout.Split('\n')[0].Trim() : null;
        return ret.ExitCode == 0;
    }

    public static async Task DownloadFileAsync(string url, string targetDir)
    {
        using HttpClient client = new();
        var downloadStream = await client.GetStreamAsync(new Uri(url));
        var downloadFile = new FileStream(targetDir, FileMode.Create, FileAccess.Write);

        await downloadStream.CopyToAsync(downloadFile);
        await downloadFile.FlushAsync();
        downloadFile.Close();
    }

    public static void RunGitClone(
        string repoUrl, 
        bool recursive = true,
        string? branch = null, 
        string? targetDir = null)
    {
        StringBuilder arguments = new();
        arguments.Append("clone");
        
        if(recursive)
            arguments.Append(" --recursive");
        if (branch != null)
            arguments.Append($" -b {branch}");
        
        Process process = new()
        {
            StartInfo =
                new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $" {arguments} {targetDir ?? ""}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
        };
        process.OutputDataReceived += (_, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (_, args) => Console.WriteLine(args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }
}