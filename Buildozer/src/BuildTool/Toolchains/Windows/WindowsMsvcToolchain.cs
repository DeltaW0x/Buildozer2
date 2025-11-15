using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public sealed class WindowsMsvcToolchain : MsvcToolchainBase
    {
        private static readonly string _cCompilationRule = "C_COMPILE";
        private static readonly string _cxxCompilationRule = "CXX_COMPILE";
        private static readonly string _linkSharedLibRule = "LINK_SHARED";
        private static readonly string _linkExecutableRule = "LINK_EXECUTABLE";

        public WindowsMsvcToolchain(TargetPlatform platform, TargetArchitecture arch) : base (platform, arch) 
        {
            Name = "Windows MSVC";
            ExecutableExtension = "exe";

            Defines.AddRange([
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
                    Libraries.AddRange(["vcruntimed.lib", "ucrtd.lib"]);
                    break;
                case TargetConfiguration.Release:
                    Libraries.AddRange(["vcruntime.lib", "ucrt.lib"]);
                    break;
            }

            Libraries.AddRange([
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
                Defines.Add("USE_SOFT_INTRINSICS");
                Libraries.Add("softintrin.lib");
            }
        }

        public override void CompileCFiles(NinjaContext ninjaCtx, BuildOptions options, params string[] cxxFiles)
        {
            throw new NotImplementedException();
        }
        public override void CompileCxxFiles(NinjaContext ninjaCtx, BuildOptions options, params string[] cFile)
        {
            throw new NotImplementedException();
        }
        
        public override void LinkExecutable(NinjaContext ninjaCtx, BuildOptions options, string outputName, params string[] objectFiles)
        {
            throw new NotImplementedException();
        }
        public override void LinkSharedLibrary(NinjaContext ninjaCtx, BuildOptions options, string outputName, params string[] objectFiles)
        {
            throw new NotImplementedException();
        }

        public override void SetupNinjaToolchain(NinjaContext ninjaContext)
        {
            throw new NotImplementedException();
        }

        public override bool Validate(out string? message)
        {
            throw new NotImplementedException();
        }
    }
}
