using System.Collections.Generic;
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

        public string CompilerName { get; set; }
        public string LinkerName { get; set; }
        public string LibrarianName { get; set; }
        public string StripperName { get; set; }

        public string CompilerBinDirectory { get; set; }

        public string LibraryNamePrefix { get; set; }
        public string SharedLibExtension { get; set; }
        public string StaticLibExtension { get; set; }
        public string ExecutableExtension { get; set; }
        public string ObjectFileExtension { get; set; }

        public string SharedLibImportDefine { get; set; }
        public string SharedLibExportDefine { get; set; }

        public string CdeclCallingDefine { get; set; }

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
        }

        public abstract bool Validate(out string? message);

        public abstract void SetupNinjaToolchain(NinjaContext ninjaContext);

        public virtual void OnPreBuild(NinjaContext ninjaCtx, BuildOptions options) { }
        public virtual void OnPostBuild(NinjaContext ninjaCtx, BuildOptions options) { }

        public abstract void CompileCFiles(NinjaContext ninjaCtx, BuildOptions options, params string[] cxxFiles);
        public abstract void CompileCxxFiles(NinjaContext ninjaCtx, BuildOptions options, params string[] cFile);
        
        public abstract void LinkSharedLibrary(NinjaContext ninjaCtx, BuildOptions options, string outputName, params string[] objectFiles);
        public abstract void LinkExecutable(NinjaContext ninjaCtx, BuildOptions options, string outputName, params string[] objectFiles);
    }
}
