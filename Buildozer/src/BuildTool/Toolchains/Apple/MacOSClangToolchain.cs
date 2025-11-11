using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class MacOSClangToolchain : ClangToolchainBase
    {
        public MacOSClangToolchain(OSPlatform toolchainPlatform, Architecture toolchainArchitecture, Version compilerVersion) 
            : base(toolchainPlatform, toolchainArchitecture, compilerVersion)
        {
        }
    }
}
