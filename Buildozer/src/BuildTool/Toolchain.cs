using Serilog;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Buildozer.BuildTool
{
    public class Toolchain
    {
        public string Name { get; protected set; } = "";
        public string Description { get; protected set; } = "";

        public OSPlatform ToolchainPlatform { get; protected set; }
        public Architecture ToolchainArchitecture { get; protected set; }

        public bool IsCrossCompiler { get; protected set; } = false;
        
        public string CompilerRoot { get; protected set; } = "";
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
                throw new NotImplementedException("Linux toolchain discovery not implemented yet");
                //return DiscoverLinuxToolchains();
            }
            if (OperatingSystem.IsMacOS())
            {
                throw new NotImplementedException("Linux toolchain discovery not implemented yet");
                //return DiscoverMacOSToolchains();
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

                if (winSdkPath == null)
                {
                    Log.Error("Failed to find a suitable Windows SDK installation. Please install the Windows SDK and try again");
                    return [];
                }
                if (winSdkVersion == null || winSdkVersion < BuildContext.MiniumWinSdkVersion)
                {
                    Log.Error("Failed to find a suitable Windows SDK version. Please install the Windows SDK and try again");
                    return [];
                }

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
                        string clangPath = Path.Join(vsInstance.GetInstallationPath(), "VC", "Tools", "Llvm");

                        if (Directory.Exists(msvcPath))
                        {
                            foreach (var msvcDir in Directory.GetDirectories(msvcPath))
                            {
                                Version msvcVer = new Version(new DirectoryInfo(msvcDir).Name);
                                if (msvcVer < BuildContext.MinimumMsvcVersion)
                                    continue;
                                
                                if(RuntimeInformation.OSArchitecture == Architecture.X64)
                                {
                                    if (!Directory.Exists(Path.Join(msvcDir, "bin", "Hostx64", "x64")))
                                        continue;

                                    if (Directory.Exists(Path.Join(msvcDir, "bin", "Hostx64", "arm64")))
                                    {

                                    }
                                }
                                if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                                {
                                    if (!Directory.Exists(Path.Join(msvcDir, "bin", "Hostarm64", "arm64")))
                                        continue;

                                    if (Directory.Exists(Path.Join(msvcDir, "bin", "Hostarm64", "x64")))
                                    {

                                    }
                                }
                            }
                        }
                        if(Directory.Exists(clangPath))
                        {

                        }
                    }
                }
                while (fetched > 0);
            }
            catch (COMException ex)
            {
                Log.Error($"Error 0x{ex.HResult:x8}: {ex.Message}");
                return [];
            }
            catch (Exception ex)
            {
                Log.Error("The Visual Studio Setup Configuration API is not registered. Assuming no Visual Studio instances are installed, please do install one.");
                return [];
            }

            return toolchains.ToArray();
        }

        [SupportedOSPlatform("linux")]
        private static Toolchain[] DiscoverLinuxToolchains()
        {
            List<Toolchain> toolchains = new List<Toolchain>();

            Utils.CommandReturn cmdReturn = Utils.RunCommand("sh", "which");
            if (cmdReturn.ExitCode == 127)
            {
                Log.Error("Failed to find the 'which' command line utility, please install it before using this software");
                return [];
            }
            
            cmdReturn = Utils.RunCommand("sh", "-c \"grep\"");
            if (cmdReturn.ExitCode == 127)
            {
                Log.Error("Failed to find the 'grep' command line utility, please install it before using this software");
                return [];
            }
            
            cmdReturn = Utils.RunCommand("sh", "-c \"which clang\"");
            if (cmdReturn.ExitCode == 1)
            {
                Log.Error("Failed to find the Clang compiler suite, please install Clang 20.0.0+ before using this software");
                return [];
            }

            string clangPath = cmdReturn.Stdout.Trim();
            cmdReturn = Utils.RunCommand($"{clangPath}", $"-dM -E - < /dev/null");
            cmdReturn = Utils.RunCommand($"sh", $"-c \"grep {cmdReturn.Stdout} -E '__clang_(major|minor|patchlevel)__'\"");
            cmdReturn = Utils.RunCommand($"sh", $"-c \"sort {cmdReturn.Stdout}\"");

            Version clangVer = Utils.ParseClangVersionStdout(cmdReturn.Stdout);
            if (clangVer < BuildContext.MinimumClangVersion)
            {
                Log.Error($"Failed to find a suitable Clang installation version (found {clangVer}), please install Clang 20.0.0+ before using this software");
                return [];
            }

            return toolchains.ToArray();
        }

        [SupportedOSPlatform("macos")]
        private static Toolchain[] DiscoverMacOSToolchains()
        {
            return [];
        }
    }
}
