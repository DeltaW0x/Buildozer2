namespace Buildozer.BuildTool;

public class ClangToolchain : Toolchain
{
    public ClangToolchain(string name, TargetPlatform platform, TargetArchitecture architecture, Version version) : base(name, platform, architecture, version)
    {
        CompilerName = "clang";
        LinkerName = "lld";
        LibrarianName = "ar";
        ObjectFileExtension = "o";
        StaticLibExtension = ".a";
    }
}

