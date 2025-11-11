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
        public MsvcToolchainBase(OSPlatform toolchainPlatform, Architecture toolchainArchitecture, Version compilerVersion) 
            : base(toolchainPlatform, toolchainArchitecture, compilerVersion)
        {
            CompilerName = "cl.exe";
            LinkerName = "link.exe";
            LibrarianName = "lib.exe";

            SharedLibExtension = "dll";
            StaticLibExtension = "lib";
            ObjectFileExtension = "obj";

            HasImportLibs = true;

            CFlags.Add($"/std:{BuildContext.CurrentCVersion switch
            {
                BuildLanguageVersion.C11 => "c11",
                _ => throw new ArgumentException("Invalid C version")
            }}");

            CXXFlags.AddRange([
                "/Zc:__cplusplus",
                $"/EHsc{(BuildContext.EnableExceptions ? "" : "-")}",
                $"/std:{BuildContext.CurrentCxxVersion switch
                    {
                        BuildLanguageVersion.Cxx17 => "c++17",
                        BuildLanguageVersion.Cxx20 => "c++20",
                        _ => throw new ArgumentException("Invalid C++ version")
                    }
                }",
                $"{BuildContext.CurrentWarningLevel switch
                    {
                        BuildWarningLevel.W0 => "/W0",
                        BuildWarningLevel.W1 => "/W1",
                        BuildWarningLevel.W2 => "/W2",
                        BuildWarningLevel.W3 => "/W3",
                        _ => throw new ArgumentException("Invalid compiler warning level")
                    }
                }"
            ]);

            if (!BuildContext.EnableExceptions)
                Defines.Add("_HAS_EXCEPTIONS=0");

            LibrarianOptions.Add("/nologo");
            CompilerOptions.AddRange(["/nologo", "/showIncludes", "/utf-8", "/GS", "/Zi"]);
            LinkerOptions.AddRange(["/nologo", "/DEBUG", $"/MACHINE:{(ToolchainArchitecture == Architecture.X64 ? "X64" : "ARM64")}"]);

            switch (BuildContext.CurrentBuildConfig)
            {
                case BuildConfig.Debug:
                    CompilerOptions.AddRange(["/MDd", "/Od", "/Oy-", "/RTC1"]);
                    LinkerOptions.AddRange(["/INCREMENTAL:NO"]);
                    break;
                case BuildConfig.Develop:
                    CompilerOptions.AddRange(["/MDd", "/O1", "/Oy-"]);
                    break;
                case BuildConfig.Release:
                    CompilerOptions.AddRange(["/MD", "/O2", "/GL"]);
                    LinkerOptions.AddRange(["/INCREMENTAL:NO", "/LTCG", "/OPT:REF", "/OPT:ICF"]);
                    LibrarianOptions.Add("/LTCG");
                    break;
            }
        }

        public override bool HasHeader(string header)
        {
            string filePath = Path.Join(Path.GetTempPath(), "check_header.cpp");
            string objPath = Path.Join(Path.GetTempPath(), "check_header.obj");
            File.WriteAllText(filePath, $"#include <{header}>\n int main(){{return 0;}}");
            var res = Utils.RunProcess(Path.Join(BinRoot, CompilerName), $"/nologo /c /std:{BuildContext.CurrentCxxVersion} {String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""))}  \"{filePath}\" /Fo\"{objPath}\"");
            return res.ExitCode == 0;
        }

        public override string GenerateNinjaToolchain()
        {
            StringBuilder ninjaFile = new StringBuilder();
            ninjaFile.AppendLine("msvc_deps_prefix = Note: including file: ");
            ninjaFile.AppendLine();
            ninjaFile.AppendLine(GenerateCompileRule("COMPILER"));
            ninjaFile.AppendLine(GenerateLinkRule("LINKER"));
            ninjaFile.AppendLine(GenerateArchiveRule("ARCHIVER"));
            return ninjaFile.ToString();
        }

        public override string GenerateNinjaCCompilationCommand(string source)
        {
            string depFile = $"{Path.GetFileNameWithoutExtension(source)}.{ObjectFileExtension}.d";

            string compilerOptions = String.Join(" ", CompilerOptions);
            string cFlags = String.Join(" ", CFlags);
            string includeDirs = String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""));
            string definitions = String.Join(" ", Defines.Select(d => $"/D{d}"));

            StringBuilder buildCommand = new StringBuilder();
            buildCommand.AppendLine($"build {Path.GetFileNameWithoutExtension(source)}.{ObjectFileExtension}: COMPILER {source}");
            buildCommand.AppendLine($"  DEPFILE = \"{depFile}\"");
            buildCommand.AppendLine($"  DEPFILE_UNQUOTED = {depFile}");
            buildCommand.AppendLine($"  ARGS = {compilerOptions} {cFlags} {includeDirs} {definitions}");
            buildCommand.AppendLine();
            return buildCommand.ToString();
        }

        public override string GenerateNinjaCxxCompilationCommand(string source)
        {
            string depFile = $"{Path.GetFileNameWithoutExtension(source)}.{ObjectFileExtension}.d";

            string compilerOptions = String.Join(" ", CompilerOptions);
            string cxxFlags = String.Join(" ", CXXFlags);
            string includeDirs = String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""));
            string definitions = String.Join(" ", Defines.Select(d => $"/D{d}"));

            StringBuilder buildCommand = new StringBuilder();
            buildCommand.AppendLine($"build {Path.GetFileNameWithoutExtension(source)}.{ObjectFileExtension}: COMPILER {source}");
            buildCommand.AppendLine($"  DEPFILE = \"{depFile}\"");
            buildCommand.AppendLine($"  DEPFILE_UNQUOTED = {depFile}");
            buildCommand.AppendLine($"  ARGS = {compilerOptions} {cxxFlags} {includeDirs} {definitions}");
            buildCommand.AppendLine();
            return buildCommand.ToString();
        }

        public override string GenerateNinjaLinkCommand(bool sharedLib, string outName, params string[] objects)
        {
            string linkerOptions = String.Join(" ", LinkerOptions);
            string libraryDirs = String.Join(" ", LibraryDirs.Select(dir => $"/LIBPATH:\"{dir}\""));
            string libraries = String.Join(" ", Libraries);
            string files = String.Join(" ", objects);

            StringBuilder linkCommand = new StringBuilder();
            linkCommand.AppendLine($"build {outName}.{(sharedLib ? SharedLibExtension : ExecutableExtension)}: LINKER {files}");
            linkCommand.AppendLine($"  ARGS = {linkerOptions} {libraryDirs} {libraries}");
            linkCommand.AppendLine();
            return linkCommand.ToString();
        }

        private string GenerateCompileRule(string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine($"  command = \"{Path.Join(BinRoot, CompilerName)}\" $ARGS $CFLAGS $CXXFLAGS /Fo$out /c $in");
            rule.AppendLine($"  deps = msvc");
            rule.AppendLine($"  description = Compiling C++ object $out");
            return rule.ToString();
        }

        private string GenerateLinkRule(string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine($"  command = \"{Path.Join(BinRoot, LinkerName)}\" $ARGS /OUT:$out $in");
            rule.AppendLine($"  description = Linking target $out");
            return rule.ToString();
        }

        private string GenerateArchiveRule(string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine($"  command = \"{Path.Join(BinRoot, LibrarianName)}\" $ARGS /OUT:$out $in");
            rule.AppendLine($"  description = Archiving target $out");
            return rule.ToString();
        }
    }
}
