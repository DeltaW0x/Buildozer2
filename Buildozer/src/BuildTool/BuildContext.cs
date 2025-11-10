namespace Buildozer.BuildTool;

public enum BuildLanguage
{
    C,
    Cxx,
    NetCore,
    NetCoreAot
}

public enum BuildConfig
{
    Debug,
    Develop,
    Release
}

public enum BuildLinkType
{
    Shared,
    Executable
}

public static class BuildContext
{
    public static Version MinimumVsVersion { get; } = new Version(17, 0, 0);
    public static Version MinimumMsvcVersion { get; } = new Version(14, 38, 0);
    public static Version MiniumWinSdkVersion { get; } = new Version(10, 0, 18362, 0);

    public static Version MinimumClangVersion { get; } = new Version(20, 0, 0);
    public static Version MinimumXcodeVersion { get; } = new Version(16, 3 ,0);

    public static BuildConfig CurrentConfig { get; set; } = BuildConfig.Develop;

    public static bool EnableExceptions => false;

    public static string CurrentCVersion => "c11";
    public static string CurrentCxxVersion => "c++20";
}
