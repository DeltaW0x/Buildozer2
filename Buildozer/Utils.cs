using System.Diagnostics;

namespace Buildozer;

public static class Utils
{
    public static void RunCommand(string shell, string command, out string output, out string error, out int exitCode)
    {
        var process = new Process();
        process.StartInfo.FileName = shell;
        process.StartInfo.Arguments = OperatingSystem.IsWindows() ? $"{command}" : $"""-c "{command}" """;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.Start();
        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        exitCode = process.ExitCode;
    }
}