namespace Buildozer.BuildTool;

public enum BuildWarningLevel
{
    W0,
    W1,
    W2,
    W3,
    W4,
    Wall
}

public enum BuildLanguage
{
    C,
    Cxx,
    NetCore,
    NetCoreAot
}

public enum BuildLanguageVersion
{
    C11,
    Cxx17,
    Cxx20,
    Net9,
    Net10,
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
    public static Version MinimumVisualStudioVersion { get; } = new Version(17, 0, 0);
    public static Version MinimumMsvcVersion { get; } = new Version(14, 38, 0);
    public static Version MiniumWindowsSdkVersion { get; } = new Version(10, 0, 18362, 0);

    public static Version MinimumGCCVersion { get; } = new Version(13, 0, 0);

    public static Version MinimumClangVersion { get; } = new Version(17, 0, 0);
   
    public static Version MinimumXcodeVersion { get; } = new Version(16, 3 ,0);

    public static BuildLanguageVersion CurrentCVersion => BuildLanguageVersion.C11;
    public static BuildLanguageVersion CurrentCxxVersion => BuildLanguageVersion.Cxx20;

    public static bool EnableExceptions = false;
    public static BuildConfig CurrentBuildConfig = BuildConfig.Release;
    public static BuildWarningLevel CurrentWarningLevel = BuildWarningLevel.W3;
}
