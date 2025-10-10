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

        public Version CompilerVersion { get; protected set; } = new();

        public string CompilerName { get; protected set; } = "";
        public string LinkerName { get; protected set; } = "";
        public string LibrarianName { get; protected set; } = "";
        public string StripperName { get; protected set; } = "";

        public string LibNamePrefix { get; protected set; } = "";
        public string SharedLibExtension { get; protected set; } = "";
        public string StaticLibExtension { get; protected set; } = "";
        public string ObjectFileExtension { get; protected set; } = "";

        public List<string> Definitions { get; protected set; } = new();

        public List<string> IncludeDirs { get; protected set; } = new();
        public List<string> LibraryDir { get; protected set; } = new();

        public List<string> CompilerOptions { get; protected set; } = new();
        public List<string> LinkerOptions { get; protected set; } = new();
        public List<string> LibrarianOptions { get; protected set; } = new();
        public List<string> StripperOptions { get; protected set; } = new();

        public List<string> Libraries { get; protected set; } = new();

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
                        if (vsVersion.Major != BuildContext.MinimumVsVersion.Major)
                            continue;

                        string msvcPath = Path.Join(vsInstance.GetInstallationPath(), "VC", "Tools", "MSVC");
                        if (!Directory.Exists(msvcPath))
                            continue;

                        foreach (var msvcDir in Directory.GetDirectories(msvcPath))
                        {
                            Version msvcVer = new Version(new DirectoryInfo(msvcDir).Name);
                            if (msvcVer < BuildContext.MinimumMsvcVersion)
                                continue;

                            if (Directory.Exists(Path.Join(msvcDir, "bin", "Hostx64", "x64")))
                            {
                                var toolchain = new MsvcToolchain("Windows x64", vsVersion, msvcDir, msvcVer, winSdkPath, winSdkVersion, BuildArchitecture.X64, false);
                                toolchains.Add(toolchain);
                            }
                            if (Directory.Exists(Path.Join(msvcDir, "bin", "Hostx64", "arm64")))
                            {
                                var toolchain = new MsvcToolchain("Windows arm64", vsVersion, msvcDir, msvcVer, winSdkPath, winSdkVersion, BuildArchitecture.Arm64, true);
                                toolchains.Add(toolchain);
                            }
                            if (Directory.Exists(Path.Join(msvcDir, "bin", "Hostarm64", "arm64")))
                            {
                                var toolchain = new MsvcToolchain("Windows arm64", vsVersion, msvcDir, msvcVer, winSdkPath, winSdkVersion, BuildArchitecture.Arm64, false);
                                toolchains.Add(toolchain);
                            }
                            if (Directory.Exists(Path.Join(msvcDir, "bin", "Hostarm64", "x64")))
                            {
                                var toolchain = new MsvcToolchain("Windows x64", vsVersion, msvcDir, msvcVer, winSdkPath, winSdkVersion, BuildArchitecture.X64, true);
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
