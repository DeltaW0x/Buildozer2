using Microsoft.Win32;
using Microsoft.VisualStudio.Setup.Configuration;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Buildozer.BuildTool
{
    public class Toolchain
    {
        public string Name { get; protected set; } = "";
        public string Description { get; protected set; } = "";

        public BuildPlatform ToolchainPlatform { get; protected set; }
        public BuildArchitecture ToolchainArchitecture { get; protected set; }

        public bool IsCrossCompiler { get; protected set; } = false;

        public string SdkRoot { get; protected set; } = "";

        public Version CompilerVersion { get; protected set; } = new();

        public string CompilerName { get; protected set; } = "";
        public string LinkerName { get; protected set; } = "";
        public string LibrarianName { get; protected set; } = "";
        public string StripperName { get; protected set; } = "";

        public string LibNamePrefix { get; protected set; } = "";
        public string SharedLibExtension { get; protected set; } = "";
        public string StaticLibExtension { get; protected set; } = "";
        public string ObjectFileExtension { get; protected set; } = "";

        public List<string> GlobalDefines { get; protected set; } = new();

        public List<string> SystemIncludeDirs { get; protected set; } = new();
        public List<string> SystemLibraryDirs { get; protected set; } = new();

        public List<string> GlobalCompilerOptions { get; protected set; } = new();
        public List<string> GlobalLinkerOptions { get; protected set; } = new();
        public List<string> GlobalLibrarianOptions { get; protected set; } = new();
        public List<string> GlobalStripperOptions { get; protected set; } = new();
        public List<string> GlobalLinkedLibraries { get; protected set; } = new();

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
            throw new PlatformNotSupportedException("Current host platform isn't supported");
        }

        [SupportedOSPlatform("windows")]
        private static Toolchain[] DiscoverWindowsToolchains()
        {
            List<Toolchain> toolchains = new List<Toolchain>();

            string? winSdkPath = null;
            Version? winSdkVersion = null;

            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Microsoft SDKs\\Windows\\v10.0"))
                {
                    if (key != null)
                    {
                        object? version = key.GetValue("ProductVersion");
                        if (version != null)
                        {
                            winSdkVersion = new Version(version.ToString()!);
                        }

                        object? installPath = key.GetValue("InstallationFolder");
                        if (installPath != null)
                        {
                            winSdkPath = installPath.ToString()!;
                        }
                    }
                }

                _ = winSdkPath ?? throw new Exception("Failed to query thw Windows SDK path");
                _ = winSdkVersion ?? throw new Exception("Failed to query the Windows SDK version");

                var vsQuery = (ISetupConfiguration2)(new SetupConfiguration());
                var e = vsQuery.EnumAllInstances();

                int fetched;
                var instances = new ISetupInstance[1];
                do
                {
                    e.Next(1, instances, out fetched);
                    if (fetched > 0)
                    {
                        var vsInstance = (ISetupInstance2)instances[0];

                        Version vsVersion = new(vsInstance.GetInstallationVersion());
                        if (vsVersion < BuildContext.MinimumVsVersion)
                            continue;

                        string msvcPath = Path.Join(vsInstance.GetInstallationPath(), "VC", "Tools", "MSVC");
                        if (!Directory.Exists(msvcPath))
                            continue;

                        foreach (var dir in Directory.GetDirectories(msvcPath))
                        {
                            Version msvcVer = new Version(new DirectoryInfo(dir).Name);
                            if (msvcVer < BuildContext.MinimumMsvcVersion)
                                continue;

                            string x64_x64 = Path.Join(dir, "bin", "Hostx64", "x64");   // X64 -> X64 Compiler
                            string x64_arm64 = Path.Join(dir, "bin", "Hostx64", "arm64");   // X64 -> Arm64 CrossCompiler

                            string arm64_arm64 = Path.Join(dir, "bin", "Hostarm64", "arm64");   // Arm64 -> Arm64 Compiler
                            string arm64_x64 = Path.Join(dir, "bin", "Hostarm64", "x64");   // Arm64 -> X64 CrossCompiler

                            if (Directory.Exists(x64_x64))
                            {
                                var toolchain = new MsvcToolchain("MSVC x64", x64_x64, msvcVer, vsVersion);
                                toolchain.IsCrossCompiler = false;
                                toolchain.ToolchainArchitecture = BuildArchitecture.X64;
                                toolchains.Add(toolchain);
                            }
                            if (Directory.Exists(x64_arm64))
                            {
                                var toolchain = new MsvcToolchain("MSVC arm64 (CrossCompiler)", x64_arm64, msvcVer, vsVersion);
                                toolchain.IsCrossCompiler = true;
                                toolchain.ToolchainArchitecture = BuildArchitecture.Arm64;
                                
                                toolchains.Add(toolchain);
                            }

                            if (Directory.Exists(arm64_arm64))
                            {
                                var toolchain = new MsvcToolchain("MSVC arm64", arm64_arm64, msvcVer, vsVersion);
                                toolchain.IsCrossCompiler = false;
                                toolchain.ToolchainArchitecture = BuildArchitecture.Arm64;
                                toolchains.Add(toolchain);
                            }
                            if (Directory.Exists(arm64_x64))
                            {
                                var toolchain = new MsvcToolchain("MSVC x64 (CrossCompiler)", arm64_x64, msvcVer, vsVersion);
                                toolchain.IsCrossCompiler = true;
                                toolchain.ToolchainArchitecture = BuildArchitecture.X64;
                                toolchains.Add(toolchain);
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

        [SupportedOSPlatform("linux")]
        private static Toolchain[] DiscoverLinuxToolchains()
        {
            return [];
        }

        [SupportedOSPlatform("macos")]
        private static Toolchain[] DiscoverMacOSToolchains()
        {
            return [];
        }
    }
}
