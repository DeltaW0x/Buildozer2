using System;
using System.Collections.Generic;
using System.Text;

namespace Buildozer.BuildTool
{
    public class BuildOptions
    {
        public readonly Toolchain Toolchain;

        public readonly TargetPlatform Platform;
        public readonly TargetArchitecture Architecture;
        public readonly TargetConfiguration Configuration;

        public readonly List<string> Sources = new List<string>();
        public readonly List<string> Outputs = new List<string>();

        public readonly HashSet<string> PublicIncludePaths = new HashSet<string>();
        public readonly HashSet<string> PrivateIncludePaths = new HashSet<string>();

        public readonly HashSet<string> PublicDefinitions = new HashSet<string>();
        public readonly HashSet<string> PrivateDefinitions = new HashSet<string>();

        public readonly HashSet<string> PublicDependencies = new HashSet<string>();
        public readonly HashSet<string> PrivateDependencies = new HashSet<string>();

        public readonly HashSet<string> PublicLibraries = new HashSet<string>();
        public readonly HashSet<string> PrivateLibraries = new HashSet<string>();
    }
}
