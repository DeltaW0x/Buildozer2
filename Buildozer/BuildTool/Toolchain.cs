using System.Diagnostics;
using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Buildozer.BuildTool;

public class Toolchain
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public Version Version { get; protected set; }

    public bool IsCrossCompiler { get; protected set; }

    public string SdkRoot { get; protected set; }
    public TargetPlatform Platform { get; protected set; }
    public TargetArchitecture Architecture { get; protected set; }

    public string LibNamePrefix { get; protected set; } = "";
    public string SharedLibExtension { get; protected set; }
    public string StaticLibExtension { get; protected set; }
    public string ObjectFileExtension { get; protected set; }

    public List<string> GlobalDefines { get; protected set; } = new();
    public List<string> GlobalCDefines { get; protected set; } = new();
    public List<string> GlobalCxxDefines { get; protected set; } = new();
    
    public List<string> SystemIncludeDirs { get; protected set; } = new();
    
    public string CompilerName { get; protected set; }
    public List<string> GlobalCompilerOptions { get; protected set; } = new();

    public string LinkerName { get; protected set; }
    public List<string> GlobalLinkerOptions { get; protected set; } = new();

    public string LibrarianName { get; protected set; }
    public List<string> GlobalLibrarianOptions { get; protected set; } = new();

    protected Toolchain(string name, TargetPlatform platform, TargetArchitecture architecture, Version version)
    {
        Name = name;
        Platform = platform;
        Architecture = architecture;
        Version = version;
    }

    public static Toolchain[] DiscoverSystemToolchains()
    {
        if (OperatingSystem.IsWindows())
        {
            return DiscoverWindowsToolchains();
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return DiscoverUnixToolchains();
        }
        throw new PlatformNotSupportedException("Current host platform isn't suppored");
    }

    [SupportedOSPlatform("windows")]
    private static Toolchain[] DiscoverWindowsToolchains()
    {
        Version sdkVersion = new();
        string sdkInstallPath = "";
        List<Toolchain> toolchains = new List<Toolchain>();
        
        try
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Microsoft SDKs\\Windows\\v10.0"))
            {
                if (key != null)
                {
                    object? version = key.GetValue("ProductVersion");
                    if (version != null)
                    {
                        sdkVersion = new Version(version.ToString()!);
                    }

                    object? installPath = key.GetValue("InstallationFolder");
                    if (installPath != null)
                    {
                        sdkInstallPath = installPath.ToString()!;
                    }
                }
            }

            var query = (ISetupConfiguration2)(new SetupConfiguration());
            var e = query.EnumAllInstances();

            int fetched;
            var instances = new ISetupInstance[1];
            do
            {
                e.Next(1, instances, out fetched);
                if (fetched > 0)
                {
                    var vsInstance = (ISetupInstance2)instances[0];
                    Version version = new(vsInstance.GetInstallationVersion());
                    
                    if (version.Major != 17)
                        continue;

                    string mvscPath = Path.Join(vsInstance.GetInstallationPath(), "VC", "Tools", "MSVC");

                    if (!Directory.Exists(mvscPath))
                        continue;

                    foreach (var dir in Directory.GetDirectories(mvscPath))
                    {
                        Version msvcVer = new Version(new DirectoryInfo(dir).Name);
                        if (msvcVer < new Version(14, 38, 33130))
                            continue;

                        string x64 = Path.Join(dir, "bin", "Hostx64", "x64");
                        string arm64 = Path.Join(dir, "bin", "Hostx64", "arm64");

                        if (RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64)
                        {
                            if (Directory.Exists(x64))
                            {
                                var toolchain = new MsvcToolchain("MSVC x64",
                                    TargetPlatform.Windows,
                                    TargetArchitecture.X64,
                                    msvcVer,
                                    x64,
                                    sdkInstallPath);
                                toolchain.IsCrossCompiler = RuntimeInformation.OSArchitecture !=
                                                            System.Runtime.InteropServices.Architecture.X64;
                                toolchains.Add(toolchain);
                            }

                            if (Directory.Exists(arm64))
                            {
                                var toolchain = new MsvcToolchain("MSVC Arm64",
                                    TargetPlatform.Windows,
                                    TargetArchitecture.Arm64,
                                    msvcVer,
                                    arm64,
                                    sdkInstallPath);
                                toolchain.IsCrossCompiler = RuntimeInformation.OSArchitecture !=
                                                            System.Runtime.InteropServices.Architecture.Arm64;
                                toolchains.Add(toolchain);
                            }
                        }
                    }
                }
            }
            while (fetched > 0);
        }
        catch (COMException ex)
        {
            Console.WriteLine("The query API is not registered. Assuming no Visual Studio instances are installed.");
            return [];
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error 0x{ex.HResult:x8}: {ex.Message}");
        }

        return toolchains.ToArray();
    }

    private static void RunCommand(string executable, string command, out string output, out string error, out int exitCode)
    {
        var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.Arguments = $"""-c "{command}" """;
        //process.StartInfo.Arguments = "-c \"xcode-select --print-path\"";
        //process.StartInfo.Arguments = $"""-c "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang -dM -E -xc /dev/null | grep -e __clang_major__ -e __clang_minor__ -e __clang_patchlevel__" """;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        
        process.Start();
        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        exitCode = process.ExitCode;
    }
    
    private static Toolchain[] DiscoverUnixToolchains()
    {
        List<Toolchain> toolchains = new();

        if (OperatingSystem.IsMacOS())
        {
            string error;
            int exitCode;
            RunCommand("zsh", "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang -dM -E -xc /dev/null | grep -e __clang_major__ -e __clang_minor__ -e __clang_patchlevel__", out string stdout, out error, out exitCode);
            string[][] lines = stdout
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToArray();
            Version clangVer = new Version(int.Parse(lines[0][^1]), int.Parse(lines[1][^1]), int.Parse(lines[2][^1]));
            
            RunCommand("zsh", "xcrun --sdk iphoneos --show-sdk-path", out string iphonesdkPath, out error, out exitCode);
            Console.WriteLine(iphonesdkPath);
        }
        return toolchains.ToArray();
    }
}
