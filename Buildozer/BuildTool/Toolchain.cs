using Microsoft.Win32;
using Microsoft.VisualStudio.Setup.Configuration;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

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

        if (OperatingSystem.IsLinux())
        {
            return DiscoverLinuxToolchains();
        }

        if (OperatingSystem.IsMacOS())
        {
            return DiscoverMacOSToolchains();
        }

        Console.WriteLine("Current host platform isn't supported");
        return [];
    }
    
    [SupportedOSPlatform("windows")]
    private static Toolchain[] DiscoverWindowsToolchains()
    {
        List<Toolchain> toolchains = new List<Toolchain>();
        Architecture hostArch = RuntimeInformation.OSArchitecture;
        
        Version? sdkVersion = null;
        string? sdkInstallPath = null;
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

            if (sdkVersion == null || sdkInstallPath == null)
            {
                Console.WriteLine("Failed to find the Windows SDK");
                return [];
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
            } while (fetched > 0);
        }
        catch (COMException ex)
        {
            Console.WriteLine("The query API is not registered. Assuming no Visual Studio instances are installed.");
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error 0x{ex.HResult:x8}: {ex.Message}");
        }

        return toolchains.ToArray();
    }

    private static Version ParseClangVersion(string stdout)
    {
        string[][] lines = stdout
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        return new Version(int.Parse(lines[0][^1]), int.Parse(lines[1][^1]), int.Parse(lines[2][^1]));
    }
    
    [SupportedOSPlatform("macos")]
    private static Toolchain[] DiscoverMacOSToolchains()
    {
        List<Toolchain> toolchains = new List<Toolchain>();
        Architecture hostArch = RuntimeInformation.OSArchitecture;
        
        Utils.RunCommand("sh", "xcodebuild -version", out string xcodeVerStdout, out string _, out int exitCode);
        if (exitCode == 127)
        {
            Console.WriteLine("Failed to run command xcodebuild. Make sure that Xcode 16.3 or newer is installed");
            return [];
        }

        Version xcodeVer = new Version(xcodeVerStdout
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray()[0][1]);
        if (xcodeVer < new Version(16, 3))
        {
            Console.WriteLine("Found unsupported Xcode version. You need Xcode 16.3 or newer to use this program");
            return [];
        }
        
        return toolchains.ToArray();
    }
    
    private static Toolchain[] DiscoverLinuxToolchains()
    {
        List<Toolchain> toolchains = new();
        
        Utils.RunCommand("sh", "which", out string _, out string _, out int whichCode);
        if (whichCode == 127)
        {
            Console.WriteLine("Failed to find which. Please install the 'which' command line utility to use this program");
            return [];
        }
        
        Utils.RunCommand("sh", "grep", out string _, out string _, out int grepCode);
        if (grepCode == 127)
        {
            Console.WriteLine("Failed to find grep. Please install the 'grep' command line utility to use this program");
            return [];
        }
        
        Utils.RunCommand("sh", "which clang", out string clangPath, out string _, out int clangCode);
        if (clangCode != 0)
        {
            Console.WriteLine("Clang compiler not found, please install Clang 20.0.0+");
            return [];
        }
        clangPath = clangPath.Trim();
        
        Utils.RunCommand("sh", 
            $"{clangPath} -dM -E - < /dev/null | grep -E '__clang_(major|minor|patchlevel)__' | sort", 
            out string clangVerString, 
            out string _, 
            out int _);
        Version clangVer = ParseClangVersion(clangVerString);
        if (clangVer < new Version(20, 0, 0))
        {
            Console.WriteLine("Found unsupported clang version. You need clang 20.0.0 or newer to use this program");
            return [];
        }
        
        return toolchains.ToArray();
    }
}