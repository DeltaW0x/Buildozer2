using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.Win32;
using Serilog;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json.Serialization;

namespace Buildozer.BuildTool
{
    [JsonDerivedType(typeof(WindowsMsvcToolchain), "windows_msvc_toolchain")]
    [JsonDerivedType(typeof(LinuxClangToolchain), "linux_clang_toolchain")]
    [JsonDerivedType(typeof(MacOSClangToolchain), "macos_clang_toolchain")]
    [JsonDerivedType(typeof(IphoneOSClangToolchain), "ios_clang_toolchain")]
    public abstract class Toolchain
    {
        public string Name { get; protected set; } = "";
        public string Description { get; protected set; } = "";

        public OSPlatform ToolchainPlatform { get; protected set; }
        public Architecture ToolchainArchitecture { get; protected set; }

        public bool IsCrossCompiler { get; protected set; } = false;
        public bool HasASan { get; set; } = false;
        public bool HasImportLibs { get; protected set; } = false;
        
        public Version CompilerVersion { get; protected set; } = new();

        public string CompilerName { get; protected set; } = "";
        public string LinkerName { get; protected set; } = "";
        public string LibrarianName { get; protected set; } = "";
        public string StripperName { get; protected set; } = "";

        public string LibNamePrefix { get; protected set; } = "";
        public string SharedLibExtension { get; protected set; } = "";
        public string StaticLibExtension { get; protected set; } = "";
        public string ExecutableExtension { get; protected set; } = "";
        public string ObjectFileExtension { get; protected set; } = "";

        public List<string> Defines { get; set; } = new();
        public List<string> Libraries { get; set; } = new();

        public string BinaryDir { get; protected set; } = "";
        public List<string> IncludeDirs { get; set; } = new();
        public List<string> LibraryDirs { get; set; } = new();

        public List<string> CFlags { get; set; } = new();
        public List<string> CXXFlags { get; set; } = new();
        public List<string> CompilerOptions { get; set; } = new();
        public List<string> LinkerOptions { get; set; } = new();
        public List<string> LibrarianOptions { get; set; } = new();
        public List<string> StripperOptions { get; set; } = new();

        public static Toolchain[] DiscoverSystemToolchains()
        {
            if (OperatingSystem.IsWindows())
                return DiscoverWindowsToolchains();
            if (OperatingSystem.IsMacOS())
                return DiscoverMacOSToolchains();
            
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
                            winSdkVersion = new Version(version.ToString()! + ".0");
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
                if (winSdkVersion == null || winSdkVersion < BuildContext.MiniumWindowsSdkVersion)
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
                        if (vsVersion.Major != BuildContext.MinimumVisualStudioVersion.Major)
                            continue;

                        string msvcPath = Path.Join(vsInstance.GetInstallationPath(), "VC", "Tools", "MSVC");

                        // These are common between arm64 and x64 and msvc/clang
                        string[] winSdkIncludeDirs = {
                            Path.Combine(winSdkPath, "include", winSdkVersion.ToString(), "ucrt"),
                            Path.Combine(winSdkPath, "include", winSdkVersion.ToString(), "um"),
                            Path.Combine(winSdkPath, "include", winSdkVersion.ToString(), "shared")
                        };

                        if (Directory.Exists(msvcPath))
                        {
                            foreach (var msvcDir in Directory.GetDirectories(msvcPath))
                            {
                                Version msvcVer = new Version(new DirectoryInfo(msvcDir).Name);
                                if (msvcVer < BuildContext.MinimumMsvcVersion)
                                    continue;

                                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                                {
                                    // You need at least a host compiler
                                    if (!Directory.Exists(Path.Join(msvcDir, "bin", "Hostx64", "x64")))
                                        continue;

                                    WindowsMsvcToolchain hostToolchain = new WindowsMsvcToolchain(OSPlatform.Windows, Architecture.X64, msvcVer, winSdkVersion);
                                    hostToolchain.IsCrossCompiler = false;
                                    hostToolchain.BinaryDir = Path.Join(msvcDir, "bin", "Hostx64", "x64");
                                    hostToolchain.IncludeDirs.Add(Path.Combine(msvcDir, "include"));
                                    hostToolchain.IncludeDirs.AddRange(winSdkIncludeDirs);
                                    hostToolchain.LibraryDirs.AddRange([
                                        Path.Combine(msvcDir, "lib", "x64"),
                                        Path.Combine(winSdkPath, "lib", winSdkVersion.ToString(), "um", "x64"),
                                        Path.Combine(winSdkPath, "lib", winSdkVersion.ToString(), "ucrt", "x64")
                                    ]);
                                    hostToolchain.CompilerOptions.Add("/arch:SSE4.2");
                                    if(File.Exists(Path.Join(msvcDir, "lib", "x64", "clang_rt.asan_dbg_dynamic-x86_64.lib")) &&
                                       File.Exists(Path.Join(msvcDir, "lib", "x64", "clang_rt.asan_dynamic_runtime_thunk-x86_64.lib")))
                                    {
                                        hostToolchain.HasASan = true;
                                        if(BuildContext.CurrentBuildConfig == BuildConfig.Debug)
                                        {
                                            hostToolchain.CompilerOptions.Add("/fsanitize=address");
                                            hostToolchain.Libraries.AddRange([
                                                "clang_rt.asan_dbg_dynamic-x86_64.lib",
                                                "clang_rt.asan_dynamic_runtime_thunk-x86_64.lib"
                                            ]);
                                        }
                                    }
                                    toolchains.Add(hostToolchain);
                                }
                            }
                        }
                    }
                }
                while (fetched > 0);
            }
            catch (COMException ex)
            {
                Log.Error("The Visual Studio Setup Configuration API is not registered. Assuming no Visual Studio instances are installed, please do install one.");
                Log.Error($"Error 0x{ex.HResult:x8}: {ex.Message}");
                return [];
            }

            return toolchains.ToArray();
        }

        [SupportedOSPlatform("linux")]
        private static Toolchain[] DiscoverLinuxToolchains()
        {
            List<Toolchain> toolchains = new List<Toolchain>();

            Utils.CommandReturn cmdReturn = Utils.RunProcess("sh", "which");
            if (cmdReturn.ExitCode == 127)
            {
                Log.Error("Failed to find the 'which' command line utility, please install it before using this software");
                return [];
            }

            cmdReturn = Utils.RunProcess("sh", "-c \"grep\"");
            if (cmdReturn.ExitCode == 127)
            {
                Log.Error("Failed to find the 'grep' command line utility, please install it before using this software");
                return [];
            }

            cmdReturn = Utils.RunProcess("sh", "-c \"which clang\"");
            if (cmdReturn.ExitCode == 1)
            {
                Log.Error("Failed to find the Clang compiler suite, please install Clang 20.0.0+ before using this software");
                return [];
            }

            string clangPath = cmdReturn.Stdout.Trim();
            cmdReturn = Utils.RunProcess($"{clangPath}", $"-dM -E - < /dev/null");
            cmdReturn = Utils.RunProcess($"sh", $"-c \"grep {cmdReturn.Stdout} -E '__clang_(major|minor|patchlevel)__'\"");
            cmdReturn = Utils.RunProcess($"sh", $"-c \"sort {cmdReturn.Stdout}\"");

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
            Utils.CommandReturn cmdReturn;
            List<Toolchain> toolchains = new List<Toolchain>();

            if (!Utils.CheckProgram("xcrun", out string? _))
            {
                Log.Error("Could not find the xcrun program, make sure that the Xcode command line tools are installed");
                return [];
            }

            cmdReturn = Utils.RunProcess("sh", "-c \"xcrun clang -dM -E - < /dev/null\"");
            if (cmdReturn.ExitCode != 0)
            {
                Log.Error($"Failed to query clang version: {cmdReturn.Stdout}");
                return [];
            }
            Version clangVer = Utils.ParseClangVersionStdout(cmdReturn.Stdout);
            if (clangVer < BuildContext.MinimumClangVersion)
            {
                Log.Error($"Failed to find a suitable Clang installation version (found {clangVer}), please install Clang {BuildContext.MinimumClangVersion}+ before using this software");
                return [];
            }
            
            if (!Utils.CheckProgram("xcodebuild", out string? xcodebuildPath))
            {
                Log.Error("Could not find the xcodebuild program, make sure that the Xcode is installed");
                return [];
            }
            cmdReturn = Utils.RunProcess(xcodebuildPath!, "-version");
            if (cmdReturn.ExitCode != 0)
            {
                Log.Error($"Failed to query xcode version: {cmdReturn.Stdout}");
                return [];
            }
            Version xcodeVer = new Version(cmdReturn.Stdout
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .First()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Last());
            if (xcodeVer < BuildContext.MinimumXcodeVersion)
            {
                Log.Error($"Failed to find a suitable Xcode installation version (found {xcodeVer}), please install Xcode {BuildContext.MinimumXcodeVersion}+ before using this software");
                return [];
            }
            return toolchains.ToArray();
        }

        protected Toolchain()
        {
        }
        protected Toolchain(OSPlatform toolchainPlatform, Architecture toolchainArchitecture, Version compilerVersion)
        {
            ToolchainPlatform = toolchainPlatform;
            ToolchainArchitecture = toolchainArchitecture;
            CompilerVersion = compilerVersion;

            Defines.Add(ToolchainArchitecture == Architecture.X64 ? "STOMPER_ARCH_X64" : "STOMPER_ARCH_ARM64");

            switch (BuildContext.CurrentBuildConfig)
            {
                case BuildConfig.Debug:
                    Defines.AddRange(["DEBUG", "STOMPER_DEBUG"]);
                    break;
                case BuildConfig.Develop:
                    Defines.AddRange(["DEBUG", "STOMPER_DEVELOP"]);
                    break;
                case BuildConfig.Release:
                    Defines.AddRange(["NDEBUG", "STOMPER_RELEASE"]);
                    break;
            }
        }

        public abstract bool HasHeader(string name);
        public abstract string GenerateNinjaToolchain();
        public abstract string GenerateNinjaCCompilationCommand(string source);
        public abstract string GenerateNinjaCxxCompilationCommand(string source);
        public abstract string GenerateNinjaLinkCommand(bool sharedLib, string outName, params string[] objects);
    }
}
