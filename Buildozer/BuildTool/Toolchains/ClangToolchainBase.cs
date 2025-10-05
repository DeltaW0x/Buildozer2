namespace Buildozer.BuildTool;

public class ClangToolchainBase : Toolchain
{
    protected ClangToolchainBase(string name, TargetPlatform platform, TargetArchitecture architecture, Version version) : base(name, platform, architecture, version)
    {
        CompilerName = "clang";
        LinkerName = "lld";
        LibrarianName = "ar";
        ObjectFileExtension = "o";
    }
}

