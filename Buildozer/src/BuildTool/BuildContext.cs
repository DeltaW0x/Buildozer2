namespace Buildozer.BuildTool;

public enum BuildLanguage
{
    C11,
    Cpp20,
    NetCore10,
    NetCore10Aot
}

public enum BuildConfig
{
    Debug,
    Develop,
    Release
}

public static class BuildContext
{
    public static Version MinimumVsVersion { get; } = new Version(17, 0, 0);
    public static Version MinimumMsvcVersion { get; } = new Version(14, 38, 0);
    public static Version MiniumWinSdkVersion { get; } = new Version(10, 0, 18362);

    public static Version MinimumClangVersion { get; } = new Version(20, 0, 0);
    public static Version MinimumXcodeVersion { get; } = new Version(16, 3 ,0);

    public static BuildConfig CurrentConfig { get; set; } = BuildConfig.Develop;
}
