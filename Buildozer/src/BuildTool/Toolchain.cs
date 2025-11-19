using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public abstract class Toolchain
    {
        public TargetPlatform BuildPlatform { get; private set; }
        public TargetArchitecture BuildArchitecture { get; private set; }

        public string CompilerName { get; set; }
        public string LinkerName { get; set; }
        public string LibrarianName { get; set; }
        public string StripperName { get; set; }

        public string CompilerDirectory { get; set; }

        public string LibraryNamePrefix { get; set; }
        public string SharedLibExtension { get; set; }
        public string StaticLibExtension { get; set; }
        public string ExecutableExtension { get; set; }
        public string ObjectFileExtension { get; set; }

        public string CdeclSymbol { get; set; }
        public string SharedLibImportSymbol { get; set; }
        public string SharedLibExportSymbol { get; set; }

        public readonly List<string> CFlags = new();
        public readonly List<string> CXXFlags = new();

        public readonly List<string> GlobalDefines = new();
        public readonly List<string> GlobalLibraries = new();

        public readonly List<string> SystemIncludeDirs = new();
        public readonly List<string> SystemLibraryDirs = new();

        public readonly List<string> GlobalCompilerOptions = new();
        public readonly List<string> GlobalLinkerOptions = new();
        public readonly List<string> GlobalLibrarianOptions = new();
        public readonly List<string> GlobalStripperOptions = new();

        protected Toolchain(TargetPlatform platform, TargetArchitecture arch)
        {
            BuildPlatform = platform;
            BuildArchitecture = arch;

            switch (BuildArchitecture)
            {
                case TargetArchitecture.x64:
                    GlobalDefines.Add("STOMPER_ARCH_X64");
                    break;
                case TargetArchitecture.Arm64:
                    GlobalDefines.Add("STOMPER_ARCH_ARM64");
                    break;
            }

            switch (BuildContext.CurrentBuildConfig)
            {
                case TargetConfiguration.Debug:
                    GlobalDefines.AddRange(["DEBUG", "STOMPER_DEBUG"]);
                    break;
                case TargetConfiguration.Develop:
                    GlobalDefines.AddRange(["DEBUG", "STOMPER_DEVELOP"]);
                    break;
                case TargetConfiguration.Release:
                    GlobalDefines.AddRange(["NDEBUG", "STOMPER_RELEASE"]);
                    break;
            }
        }
    }
}
