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
        public MsvcToolchainBase(OSPlatform platform, Architecture arch, Version msvcVersion) : base(platform, arch)
        {
            CompilerVersion = msvcVersion;

            CompilerName = "cl.exe";
            LinkerName = "link.exe";
            LibrarianName = "lib.exe";

            SharedLibExtension = "dll";
            StaticLibExtension = "lib";
            ObjectFileExtension = "obj";

            HasImportLibs = true;

            string[] debugFlags = {
                "/MDd",
                "/Od",
                "/GS",
                "/Oy-",
                "/Zi",
                "/RTC1"
            };
            string[] developFlags = {
                "/MDd",
                "/O1",
                "/GS",
                "/Oy-",
                "/Zi"
            };
            string[] releaseFlags = {
                "/MD",
                "/O2",
                "/GS",
                "/GL",
                "/Zi",
            };

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
                $"/{BuildContext.CurrentWarningLevel switch
                    {
                        BuildWarningLevel.W0 => "W0",
                        BuildWarningLevel.W1 => "W1",
                        BuildWarningLevel.W2 => "W2",
                        BuildWarningLevel.W3 => "W3",
                        _ => throw new ArgumentException("Invalid compiler warning level")
                    }
                }"
            ]);

            if (!BuildContext.EnableExceptions)
                Definitions.Add("_HAS_EXCEPTIONS=0");

            LibrarianOptions.Add("/nologo");
            CompilerOptions.AddRange(["/nologo", "/showIncludes", "/utf-8"]);
            LinkerOptions.AddRange(["/nologo", $"/MACHINE:{(ToolchainArchitecture == Architecture.X64 ? "X64" : "ARM64")}"]);

            BuildConfigCompilerOptions[BuildConfig.Debug].AddRange(debugFlags);
            BuildConfigCompilerOptions[BuildConfig.Develop].AddRange(developFlags);
            BuildConfigCompilerOptions[BuildConfig.Release].AddRange(releaseFlags);

            BuildConfigLinkerOptions[BuildConfig.Debug].AddRange(["/INCREMENTAL:NO", "/DEBUG"]);
            BuildConfigLinkerOptions[BuildConfig.Develop].Add("/DEBUG");
            BuildConfigLinkerOptions[BuildConfig.Release].AddRange(["/INCREMENTAL:NO", "/LTCG", "/OPT:REF", "/OPT:ICF", "/DEBUG"]);

            BuildConfigLibrarianOptions[BuildConfig.Release].Add("/LTCG");
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
            return "";
        }

        public override string GenerateNinjaCxxCompilationCommand(string source)
        {
            string depFile = $"{Path.GetFileNameWithoutExtension(source)}.{ObjectFileExtension}.d";

            string compilerOptions = String.Join(" ", CompilerOptions);
            string confCompilerOptions = String.Join(" ", BuildConfigCompilerOptions[BuildContext.CurrentBuildConfig]);
            string cxxFlags = String.Join(" ", CXXFlags);
            string includeDirs = String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""));
            string definitions = String.Join(" ", Definitions.Select(d => $"/D{d}"));
            string confDefinitions = String.Join(" ", BuildConfigDefinitions[BuildContext.CurrentBuildConfig].Select(d => $"/D{d}"));

            StringBuilder buildCommand = new StringBuilder();
            buildCommand.AppendLine($"build {Path.GetFileNameWithoutExtension(source)}.{ObjectFileExtension}: COMPILER {source}");
            buildCommand.AppendLine($"  DEPFILE = \"{depFile}\"");
            buildCommand.AppendLine($"  DEPFILE_UNQUOTED = {depFile}");
            buildCommand.AppendLine($"  ARGS = {compilerOptions} {confCompilerOptions} {cxxFlags} {includeDirs} {definitions} {confDefinitions}");
            buildCommand.AppendLine();
            return buildCommand.ToString();
        }

        public override string GenerateNinjaLinkCommand(bool sharedLib, string outName, params string[] objects)
        {
            string linkerOptions = String.Join(" ", LinkerOptions);
            string confLinkerOptions = String.Join(" ", BuildConfigLinkerOptions[BuildContext.CurrentBuildConfig]);
            string libraryDirs = String.Join(" ", LibraryDirs.Select(dir => $"/LIBPATH:\"{dir}\""));
            string libraries = String.Join(" ", Libraries);
            string confLibraries = String.Join(" ", BuildConfigLibraries[BuildContext.CurrentBuildConfig]);
            string files = String.Join(" ", objects);

            StringBuilder linkCommand = new StringBuilder();
            linkCommand.AppendLine($"build {outName}.{(sharedLib ? SharedLibExtension : ExecutableExtension)}: LINKER {files}");
            linkCommand.AppendLine($"  ARGS = {linkerOptions} {confLinkerOptions} {libraryDirs} {libraries} {confLibraries}");
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

            /*
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine("  " +
                $"command=\"{Path.Join(BinRoot, CompilerName)}\" " +
                $"/c {String.Join(" ", CompilerOptions)} {String.Join(" ", BuildConfigCompilerOptions[buildConfig])} " +
                $"{String.Join(" ", CXXFlags)} " +
                $"{String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""))} " +
                $"{String.Join(" ", Definitions.Select(d => $"/D{d}"))} " +
                $"{String.Join(" ", BuildConfigDefinitions[buildConfig].Select(d => $"/D{d}"))} " +
                $"$defines $flags $includes $in /Fo$out");
            rule.AppendLine("  description= Compiling CXX file $in");
            return rule.ToString();
            */
        }

        private string GenerateLinkRule(string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine($"  command = \"{Path.Join(BinRoot, LinkerName)}\" $ARGS /OUT:$out $in");
            rule.AppendLine($"  description = Linking target $out");
            return rule.ToString();

            /*
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine("  " +
                $"command=\"{Path.Join(BinRoot, LinkerName)}\" " +
                $"{(linkType == BuildLinkType.Shared ? "/DLL" : "")} " +
                $"{String.Join(" ", LinkerOptions)} " +
                $"{String.Join(" ", BuildConfigLinkerOptions[buildConfig])} " +
                $"{String.Join(" ", LibraryDirs.Select(dir => $"/LIBPATH:\"{dir}\""))} " +
                $"{String.Join(" ", Libraries)} " +
                $"{String.Join(" ", BuildConfigLibraries[buildConfig])} " +
                $"$in /OUT:$out");
            rule.AppendLine($"  description = Linking {(linkType == BuildLinkType.Shared ? "shared library" : "executable")} $out");
            return rule.ToString();
            */
        }
        private string GenerateArchiveRule(string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine($"  command = \"{Path.Join(BinRoot, LibrarianName)}\" $ARGS /OUT:$out $in");
            rule.AppendLine($"  description = Archiving target $out");
            return rule.ToString();

            /*
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine("  " +
                $"command=\"{Path.Join(BinRoot, LibrarianName)}\" " +
                "/nologo " +
                $"{(buildConfig == BuildConfig.Release ? "/LTCG" : "")} " +
                $"$in /OUT:$out");
            rule.AppendLine($"  description = Archiving static library $out");
            return rule.ToString();
            */
        }
    }
}
