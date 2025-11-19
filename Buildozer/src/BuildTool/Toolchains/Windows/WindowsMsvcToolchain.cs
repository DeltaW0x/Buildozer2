using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public sealed class WindowsMsvcToolchain : MsvcToolchainBase
    {
        public WindowsMsvcToolchain(TargetPlatform platform, TargetArchitecture arch) : base (platform, arch) 
        {
            GlobalDefines.AddRange([
                "WIN32",
                "_WIN32",
                "__WINDOWS__",
                "WINVER=0x0A00",
                "_WIN32_WINNT=0x0A00",
                "STOMPER_PLATFORM_WIN32"
            ]);

            switch (BuildContext.CurrentBuildConfig)
            {
                case TargetConfiguration.Debug:
                case TargetConfiguration.Develop:
                    GlobalLibraries.AddRange(["vcruntimed.lib", "ucrtd.lib"]);
                    break;
                case TargetConfiguration.Release:
                    GlobalLibraries.AddRange(["vcruntime.lib", "ucrt.lib"]);
                    break;
            }

            GlobalLibraries.AddRange([
                "uuid.lib",
                "gdi32.lib",
                "ole32.lib",
                "user32.lib",
                "shell32.lib",
                "oleaut32.lib",
                "kernel32.lib",
                "comdlg32.lib",
                "advapi32.lib",
            ]);

            if(BuildArchitecture == TargetArchitecture.Arm64)
            {
                GlobalDefines.Add("USE_SOFT_INTRINSICS");
                GlobalLibraries.Add("softintrin.lib");
            }
        }
    }
}
