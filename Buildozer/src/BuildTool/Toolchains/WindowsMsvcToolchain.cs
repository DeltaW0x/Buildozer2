using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class WindowsMsvcToolchain : MsvcToolchainBase
    {
        public readonly Version WinSdkVersion;

        public WindowsMsvcToolchain(
            OSPlatform platform, 
            Architecture arch,
            Version msvcVersion, 
            Version winSdkVersion) : base (platform, arch, msvcVersion) 
        {
            Name = "Windows MSVC";

            WinSdkVersion = winSdkVersion;

            ExecutableExtension = "exe";

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

            Libraries.AddRange([
                "kernel32.lib",
                "user32.lib", 
                "shell32.lib", 
                "ole32.lib", 
                "oleaut32.lib", 
                "uuid.lib", 
                "comdlg32.lib", 
                "advapi32.lib",
                "gdi32.lib",
            ]);
            Definitions.Add("WINVER=0x0A00");
        }
    }
}
