using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

            SharedLibExtension = ".dll";
            StaticLibExtension = ".lib";
            ObjectFileExtension = ".obj";
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
                "/Zi",
                "/RTC1"
            };
            string[] releaseFlags = {
                "/MD",
                "/O2",
                "/GS",
                "/GL",
                "/Zi",
            };

            CFlags.Add($"/std:{BuildContext.CurrentCVersion}");
            CXXFlags.AddRange([
                $"/std:{BuildContext.CurrentCxxVersion}",
                $"/EHsc{(BuildContext.EnableExceptions ? "" : "-")}"
            ]);

            if (!BuildContext.EnableExceptions)
                Definitions.Add("_HAS_EXCEPTIONS=0");

            CompilerOptions.AddRange(["/nologo", "/showIncludes"]);

            BuildConfigCompilerOptions[BuildConfig.Debug].AddRange(debugFlags);
            BuildConfigCompilerOptions[BuildConfig.Develop].AddRange(developFlags);
            BuildConfigCompilerOptions[BuildConfig.Release].AddRange(releaseFlags);

            BuildConfigLinkerOptions[BuildConfig.Debug].AddRange(["/INCREMENTAL:NO", "/DEBUG"]);
            BuildConfigLinkerOptions[BuildConfig.Develop].AddRange(["/INCREMENTAL:YES", "/DEBUG"]);
            BuildConfigLinkerOptions[BuildConfig.Release].AddRange(["/INCREMENTAL:NO", "/LTCG", "/OPT:REF", "/OPT:ICF"]);

            LinkerOptions.Add($"/MACHINE:{(ToolchainArchitecture == Architecture.X64 ? "X64" : "ARM64")}");
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
            ninjaFile.AppendLine(GenerateCxxCompileRule(BuildConfig.Debug, "CXX_DEBUG_COMPILE"));
            ninjaFile.AppendLine(GenerateCxxCompileRule(BuildConfig.Develop, "CXX_DEVELOP_COMPILE"));
            ninjaFile.AppendLine(GenerateCxxCompileRule(BuildConfig.Release, "CXX_RELEASE_COMPILE"));

            ninjaFile.AppendLine(GenerateCCompileRule(BuildConfig.Debug, "C_DEBUG_COMPILE"));
            ninjaFile.AppendLine(GenerateCCompileRule(BuildConfig.Develop, "C_DEVELOP_COMPILE"));
            ninjaFile.AppendLine(GenerateCCompileRule(BuildConfig.Release, "C_RELEASE_COMPILE"));

            ninjaFile.AppendLine(GenerateLinkRule(BuildConfig.Debug, BuildLinkType.Shared, "LINK_DEBUG_SHARED"));
            ninjaFile.AppendLine(GenerateLinkRule(BuildConfig.Develop, BuildLinkType.Shared, "LINK_DEVELOP_SHARED"));
            ninjaFile.AppendLine(GenerateLinkRule(BuildConfig.Release, BuildLinkType.Shared, "LINK_RELEASE_SHARED"));

            ninjaFile.AppendLine(GenerateLinkRule(BuildConfig.Debug, BuildLinkType.Executable, "LINK_DEBUG_EXECUTABLE"));
            ninjaFile.AppendLine(GenerateLinkRule(BuildConfig.Develop, BuildLinkType.Executable, "LINK_DEVELOP_EXECUTABLE"));
            ninjaFile.AppendLine(GenerateLinkRule(BuildConfig.Release, BuildLinkType.Executable, "LINK_RELEASE_EXECUTABLE"));

            ninjaFile.AppendLine(GenerateArchiveRule(BuildConfig.Debug, "ARCHIVE_DEBUG"));
            ninjaFile.AppendLine(GenerateArchiveRule(BuildConfig.Develop, "ARCHIVE_DEVELOP"));
            ninjaFile.AppendLine(GenerateArchiveRule(BuildConfig.Release, "ARCHIVE_RELEASE"));
            return ninjaFile.ToString();
        }

        private string GenerateCxxCompileRule(BuildConfig buildConfig, string name)
        {
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
        }

        private string GenerateCCompileRule(BuildConfig buildConfig, string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine("  " +
                $"command=\"{Path.Join(BinRoot, CompilerName)}\" " +
                $"/c {String.Join(" ", CompilerOptions)} {String.Join(" ", BuildConfigCompilerOptions[buildConfig])}" +
                $"{String.Join(" ", CFlags)}" +
                $"{String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""))} " +
                $"{String.Join(" ", Definitions.Select(d => $"/D{d}"))} " +
                $"{String.Join(" ", BuildConfigDefinitions[buildConfig].Select(d => $"/D{d}"))} " +
                $"$defines $flags $includes $in /Fo$out");
            rule.AppendLine("  description = Compiling C file $in");
            return rule.ToString();
        }

        private string GenerateLinkRule(BuildConfig buildConfig, BuildLinkType linkType, string name)
        {

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
        }

        private string GenerateArchiveRule(BuildConfig buildConfig, string name)
        {
            StringBuilder rule = new StringBuilder();
            rule.AppendLine($"rule {name}");
            rule.AppendLine("  " +
                $"command=\"{Path.Join(BinRoot, LibrarianName)}\" " +
                "/nologo " +
                $"{(buildConfig == BuildConfig.Release ? "/LTCG" : "")} " +
                $"$in /OUT:$out");
            rule.AppendLine($"  description = Archiving static library $out");
            return rule.ToString();
        }
    }
}
