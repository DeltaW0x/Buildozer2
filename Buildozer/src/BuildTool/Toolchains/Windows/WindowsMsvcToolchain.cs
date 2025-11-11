using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class WindowsMsvcToolchain : MsvcToolchainBase
    {
        public Version WinSdkVersion { get; set; }

        public WindowsMsvcToolchain(
            OSPlatform toolchainPlatform, 
            Architecture toolchainArchitecture,
            Version compilerVersion, 
            Version winSdkVersion) : base (toolchainPlatform, toolchainArchitecture, compilerVersion) 
        {
            Name = "Windows MSVC";
            WinSdkVersion = winSdkVersion;
            ExecutableExtension = "exe";

            Defines.AddRange([
                "WIN32",
                "_WIN32",
                "__WINDOWS__",
                "WINVER=0x0A00",
                "STOMPER_PLATFORM_WIN32" 
            ]);

            switch (BuildContext.CurrentBuildConfig)
            {
                case BuildConfig.Debug:
                case BuildConfig.Develop:
                    Libraries.AddRange(["vcruntimed.lib","ucrtd.lib"]);
                    break;
                case BuildConfig.Release:
                    Libraries.AddRange(["vcruntime.lib", "ucrt.lib"]);
                    break;
            }

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
        }
    }
}
