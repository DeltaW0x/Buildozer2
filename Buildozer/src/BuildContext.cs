namespace Buildozer.BuildTool;

public class BuildContext
{
    public static Version MinimumVsVersion { get; } = new Version(17, 0, 0);
    public static Version MinimumMsvcVersion { get; } = new Version(14, 38, 0);

    public static Version MinimumClangVersion { get; } = new Version(20, 0, 0);

    public static Version MinimumXcodeVersion { get; } = new Version(16, 3 ,0);
}
