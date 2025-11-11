using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class LinuxClangToolchain : ClangToolchainBase
    {
        public LinuxClangToolchain(OSPlatform toolchainPlatform, Architecture toolchainArchitecture, Version compilerVersion) 
            : base(toolchainPlatform, toolchainArchitecture, compilerVersion)
        {
        }
    }
}
