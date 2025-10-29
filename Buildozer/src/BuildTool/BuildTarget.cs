using System.Runtime.InteropServices;

namespace Buildozer.src.BuildTool;

public enum BuildLanguage
{
    C11,
    Cxx20,
    NetCore9,
    NetCore9Aot
}
public enum BuildPlatform
{
    Linux,
    MacOS,
    Windows,
}

public enum BuildArchitecture
{
    X64,
    Arm64
}

public enum BuildConfig
{
    Debug,
    Develop,
    Release
}

public static class BuildExtensions
{
    public static BuildPlatform ToPlatform(this OSPlatform platform)
    {
        if (platform == OSPlatform.Windows)
        {
            return BuildPlatform.Windows;
        }
        if (platform == OSPlatform.Linux)
        {
            return BuildPlatform.Linux;
        }
        if(platform == OSPlatform.OSX)
        {
            return BuildPlatform.MacOS;
        }
        throw new PlatformNotSupportedException($"Platform {platform} is not supported");
    }

    public static BuildArchitecture ToArch(this Architecture arch) 
    {
        switch (arch)
        {
            case Architecture.X64:
                return BuildArchitecture.X64;
            case Architecture.Arm64:
                return BuildArchitecture.Arm64;
            default: 
                throw new PlatformNotSupportedException($"Architecture {arch} is not supported");
        }
    }
}