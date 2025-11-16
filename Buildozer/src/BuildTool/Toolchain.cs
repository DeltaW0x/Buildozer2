using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class NinjaContext
    {
        public readonly StringBuilder File = new();
        public TargetPlatform TargetPlatform { get; private init; }
        public TargetArchitecture BuildArchitecture { get; private init; }

        public NinjaContext(TargetPlatform platform, TargetArchitecture arch)
        {
            TargetPlatform = platform;
            BuildArchitecture = arch;
        }
    }

    public abstract class Toolchain
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public TargetPlatform BuildPlatform { get; private set; }
        public TargetArchitecture BuildArchitecture { get; private set; }

        public required string CompilerName { get; init; }
        public required string LinkerName { get; init; }
        public required string LibrarianName { get; init; }
        public required string StripperName { get; init; }

        public required string CompilerBinDirectory { get; init; }

        public required string LibraryNamePrefix { get; init; }
        public required string SharedLibExtension { get; init; }
        public required string StaticLibExtension { get; init; }
        public required string ExecutableExtension { get; init; }
        public required string ObjectFileExtension { get; init; }

        public required string SharedLibImportSymbol { get; init; }
        public required string SharedLibExportSymbol { get; init; }

        public required string CdeclCallingDefine { get; init; }

        public readonly List<string> Defines = new();
        public readonly List<string> Libraries = new();

        public readonly List<string> IncludeDirs = new();
        public readonly List<string> LibraryDirs = new();
        
        public readonly List<string> CFlags = new();
        public readonly List<string> CXXFlags = new();

        public readonly List<string> CompilerOptions = new();
        public readonly List<string> LinkerOptions = new();
        public readonly List<string> LibrarianOptions = new();
        public readonly List<string> StripperOptions = new();

        protected Toolchain(TargetPlatform platform, TargetArchitecture arch)
        {
            BuildPlatform = platform;
            BuildArchitecture = arch;

            Defines.Add(BuildArchitecture == TargetArchitecture.x64 ? "STOMPER_ARCH_X64" : "STOMPER_ARCH_ARM64");

            switch (BuildContext.CurrentBuildConfig)
            {
                case TargetConfiguration.Debug:
                    Defines.AddRange(["DEBUG", "STOMPER_DEBUG"]);
                    break;
                case TargetConfiguration.Develop:
                    Defines.AddRange(["DEBUG", "STOMPER_DEVELOP"]);
                    break;
                case TargetConfiguration.Release:
                    Defines.AddRange(["NDEBUG", "STOMPER_RELEASE"]);
                    break;
            }
        }

        public abstract bool Validate(out string? message);
        public abstract bool HasHeader(string header);

        public abstract void SetupNinjaToolchain(NinjaContext ninjaContext);

        public virtual void OnPreBuild(NinjaContext ninjaCtx, BuildOptions options) { }
        public virtual void OnPostBuild(NinjaContext ninjaCtx, BuildOptions options) { }

        public abstract void CompileCFiles(NinjaContext ninjaCtx, BuildOptions options, params string[] cxxFiles);
        public abstract void CompileCxxFiles(NinjaContext ninjaCtx, BuildOptions options, params string[] cFile);
        
        public abstract void LinkSharedLibrary(NinjaContext ninjaCtx, BuildOptions options, string outputName, params string[] objectFiles);
        public abstract void LinkExecutable(NinjaContext ninjaCtx, BuildOptions options, string outputName, params string[] objectFiles);
    }
}
