using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Buildozer.BuildTool
{
    public class ClangToolchainBase : Toolchain
    {
        public ClangToolchainBase(OSPlatform toolchainPlatform, Architecture toolchainArchitecture, Version compilerVersion) 
            : base(toolchainPlatform, toolchainArchitecture, compilerVersion)
        {
            CompilerName = "clang";
            LinkerName = "ld";
            LibrarianName = "ar";

            SharedLibExtension = "so";
            StaticLibExtension = "a";
            ObjectFileExtension = "o";
            HasImportLibs = false;

            CFlags.Add($"-std={BuildContext.CurrentCVersion switch
            {
                BuildLanguageVersion.C11 => "c11",
                _ => throw new ArgumentException("Invalid C version")
            }}");

            if (!BuildContext.EnableExceptions)
                CXXFlags.Add("-fno-exception");

            CXXFlags.AddRange([
                $"-std={BuildContext.CurrentCxxVersion switch
                    {
                        BuildLanguageVersion.Cxx17 => "c++17",
                        BuildLanguageVersion.Cxx20 => "c++20",
                        _ => throw new ArgumentException("Invalid C++ version")
                    }
                }",
                $"{BuildContext.CurrentWarningLevel switch
                    {
                        BuildWarningLevel.W0 => "-w",
                        BuildWarningLevel.W1 => "-Wall",
                        BuildWarningLevel.W2 => "-Wall -Wextra",
                        BuildWarningLevel.W3 => "-Wall -Wextra -Wpedantic",
                        _ => throw new ArgumentException("Invalid compiler warning level")
                    }
                }"
            ]);
        }

        public override string GenerateNinjaCCompilationCommand(string source)
        {
            throw new NotImplementedException();
        }

        public override string GenerateNinjaCxxCompilationCommand(string source)
        {
            throw new NotImplementedException();
        }

        public override string GenerateNinjaLinkCommand(bool sharedLib, string outName, params string[] objects)
        {
            throw new NotImplementedException();
        }

        public override string GenerateNinjaToolchain()
        {
            throw new NotImplementedException();
        }

        public override bool HasHeader(string name)
        {
            throw new NotImplementedException();
        }
    }
}
