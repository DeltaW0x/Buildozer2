using Microsoft.CodeAnalysis;

namespace Buildozer.BuildTool;

public class MsvcToolchain : Toolchain
{
    public Version VsVersion { get; private set; } = new();

    public MsvcToolchain(string name, string binPath, Version msvcVersion, Version vsVersion)
    {
        Name = name;
        SdkRoot = binPath;
        VsVersion = vsVersion;
        CompilerVersion = msvcVersion;
        ToolchainPlatform = BuildPlatform.Windows;

        CompilerName = "cl.exe";
        LinkerName = "link.exe";
        LibrarianName = "lib.exe";

        SharedLibExtension = "dll";
        StaticLibExtension = "lib";
        ObjectFileExtension = "obj";

        GlobalDefines.Add($"WINVER=0x0A00"); // Hardcoded for now because we only support Windows 10+

        GlobalLinkedLibraries.Add("shell32.lib");
        GlobalLinkedLibraries.Add("oleaut32.lib");
        GlobalLinkedLibraries.Add("kernel32.lib");
        GlobalLinkedLibraries.Add("advapi32.lib");
        GlobalLinkedLibraries.Add("user32.lib");
        GlobalLinkedLibraries.Add("delayimp.lib");
        GlobalLinkedLibraries.Add("comdlg32.lib");
        GlobalLinkedLibraries.Add("dwmapi.lib");
        GlobalLinkedLibraries.Add("ole32.lib");

        if (ToolchainArchitecture == BuildArchitecture.X64) 
        {
            GlobalCompilerOptions.Add("/arch:AVX2");
            GlobalDefines.Add("PLATFORM_ARCH_X64=1");
        }
        else if(ToolchainArchitecture == BuildArchitecture.Arm64)
        {
            GlobalDefines.Add("USE_SOFT_INTRINSICS");
            GlobalLinkedLibraries.Add("softintrin.lib");

            GlobalDefines.Add("PLATFORM_ARCH_ARM64=1");
        }
    }
}

