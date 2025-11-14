using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Buildozer.BuildTool
{
    public abstract class MsvcToolchainBase : Toolchain
    {

        public MsvcToolchainBase(TargetPlatform platform, TargetArchitecture arch) : base(platform, arch)
        {
            CompilerName = "cl.exe";
            LinkerName = "link.exe";
            LibrarianName = "lib.exe";
            CompilerBinDirectory = "Hello";
        }
    }
}
