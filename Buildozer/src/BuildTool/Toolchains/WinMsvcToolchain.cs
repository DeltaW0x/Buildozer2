using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class WinMsvcToolchain : MsvcToolchainBase
    {
        public readonly Version WinSdkVersion;

        public WinMsvcToolchain(
            OSPlatform platform, 
            Architecture arch,
            Version msvcVersion, 
            Version winSdkVersion) : base (platform, arch, msvcVersion) 
        {
            Name = "MSVC";

            WinSdkVersion = winSdkVersion;

            ExecutableExtension = "exe";

            string[] debugFlags = {
                "/MDd",
                "/Od",
                "/GS",
                "/Oy-",
                "/Zi",
                "/RTC1"
            };
            string[] releaseFlags = {
                "/MD",
                "/O2",
                "/GS",
                "/GL",
                "/Zi",
            };

            string[] debugLibs = {
                "vcruntimed.lib",
                "ucrtd.lib"
            };
            string[] releaseLibs = {
                "vcruntime.lib",
                "ucrt.lib"
            };

            Definitions.AddRange([
                "WIN32",
                "_WIN32",
                "__WINDOWS__",
                "STOMPER_PLATFORM_WIN32" 
            ]);

            BuildConfigLibraries[BuildConfig.Debug].AddRange(debugLibs);
            BuildConfigLibraries[BuildConfig.Develop].AddRange(debugLibs);
            BuildConfigLibraries[BuildConfig.Release].AddRange(releaseLibs);

            BuildConfigCompilerOptions[BuildConfig.Debug].AddRange(debugFlags);
            BuildConfigCompilerOptions[BuildConfig.Develop].AddRange(debugFlags);
            BuildConfigCompilerOptions[BuildConfig.Release].AddRange(releaseFlags);
        }
    }
}
