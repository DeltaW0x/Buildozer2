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

        public readonly HashSet<string> Sources = new HashSet<string>();
        public readonly HashSet<string> Outputs = new HashSet<string>();

        public readonly HashSet<string> PublicIncludePaths = new HashSet<string>();
        public readonly HashSet<string> PrivateIncludePaths = new HashSet<string>();

        public readonly HashSet<string> PublicDefinitions = new HashSet<string>();
        public readonly HashSet<string> PrivateDefinitions = new HashSet<string>();

        public readonly HashSet<string> PublicDependencies = new HashSet<string>();
        public readonly HashSet<string> PrivateDependencies = new HashSet<string>();

        public readonly HashSet<string> PublicLibraries = new HashSet<string>();
        public readonly HashSet<string> PrivateLibraries = new HashSet<string>();

        public readonly HashSet<string> PublicExternalLibraries = new HashSet<string>();
        public readonly HashSet<string> PrivateExternalLibraries = new HashSet<string>();

        public readonly HashSet<string> CopyRuntimeDependencies = new HashSet<string>();
    }
}
