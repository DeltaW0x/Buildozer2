using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.BuildTool
{
    public enum TargetPlatform
    {
        Window = 0,
        Linux,
        LinuxSteamRuntime
    }

    public enum TargetArchitecture
    {
        x64 = 0,
        Arm64,
        Universal
    }

    public enum TargetConfiguration
    {
        Debug = 0,
        Develop,
        Release
    }
}
