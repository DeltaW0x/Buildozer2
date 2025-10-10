using Microsoft.CodeAnalysis;

namespace Buildozer.BuildTool;

public class MsvcToolchain : Toolchain
{
    public Version VsVersion { get; private set; } = new();
    public Version WinSdkVersion { get; private set; } = new();
    public string WinSdkRoot { get; private set; } 
    public string MsvcRoot { get; private set; }

    public MsvcToolchain(string name, Version vsVersion, string msvcRoot, Version msvcVersion, string winSdkRoot, Version winSdkVersion, BuildArchitecture arch, bool crossCompiler)
    {

        Name = name;
        VsVersion = vsVersion;

        MsvcRoot = msvcRoot;
        CompilerVersion = msvcVersion;
        
        WinSdkRoot = winSdkRoot;
        WinSdkVersion = winSdkVersion;

        IsCrossCompiler = crossCompiler;

        CompilerName = "cl.exe";
        LinkerName = "link.exe";
        LibrarianName = "lib.exe";

        SharedLibExtension = "dll";
        StaticLibExtension = "lib";
        ObjectFileExtension = "obj";

        Definitions.Add($"WINVER=0x0A00"); // Hardcoded for now because we only support Windows 10+

        if (ToolchainArchitecture == BuildArchitecture.X64) 
        {
            CompilerOptions.Add("/arch:AVX2");
            Definitions.Add("PLATFORM_ARCH_X64=1");
        }
        else if(ToolchainArchitecture == BuildArchitecture.Arm64)
        {
            Definitions.Add("USE_SOFT_INTRINSICS");
            Libraries.Add("softintrin.lib");

            Definitions.Add("PLATFORM_ARCH_ARM64=1");
        }
    }
}

