namespace Buildozer.BuildTool;


public class MsvcToolchain : Toolchain
{
    public MsvcToolchain(string name, TargetPlatform platform, TargetArchitecture architecture, Version version, string binPath, string sdkPath) : base(name, platform, architecture, version)
    {
        CompilerName = "cl.exe";
        LinkerName = "link.exe";
        LibrarianName = "lib.exe";
        
        LibNamePrefix = "";
        SharedLibExtension = "dll";
        StaticLibExtension = "lib";
        ObjectFileExtension = "obj";
    }
}
