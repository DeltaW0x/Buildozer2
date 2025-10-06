namespace Buildozer.BuildTool;

public class XcodeToolchain : ClangToolchain
{
    public XcodeToolchain(string name, TargetPlatform platform, TargetArchitecture architecture, Version version)
        : base(name, platform, architecture, version)
    {
        SharedLibExtension = ".dylib";
    }
}