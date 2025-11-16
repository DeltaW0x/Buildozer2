namespace Buildozer.BuildTool
{
    public abstract class MsvcToolchainBase : Toolchain
    {
        public MsvcToolchainBase(TargetPlatform platform, TargetArchitecture arch) : base(platform, arch)
        {
            CompilerName = "cl.exe";
            LinkerName = "link.exe";
            LibrarianName = "lib.exe";

            LibraryNamePrefix = "";
            SharedLibExtension = "dll";
            StaticLibExtension = "lib";
            ObjectFileExtension = "obj";
            SharedLibImportSymbol = "__declspec(dllimport)";
            SharedLibExportSymbol = "__declspec(dllexport)";

            CdeclCallingDefine = "__cdecl";

            CFlags.Add($"/std:{BuildContext.CurrentCVersion switch
            {
                BuildLanguageVersion.C11 => "c11",
                _ => throw new InvalidBuildLanguageException("Invalid C language version selected for toolchain")
            }}");

            CXXFlags.AddRange([
                "/Zc:__cplusplus",
                $"/EHsc{(BuildContext.EnableExceptions ? "" : "-")}",
                $"/std:{BuildContext.CurrentCxxVersion switch
                {
                    BuildLanguageVersion.Cxx17 => "c++17",
                    BuildLanguageVersion.Cxx20 => "c++20",
                    _ => throw new InvalidBuildLanguageException("Invalid C++ version")
                }}",
                $"{BuildContext.CurrentWarningLevel switch
                {
                    BuildWarningLevel.W0 => "/W0",
                    BuildWarningLevel.W1 => "/W1",
                    BuildWarningLevel.W2 => "/W2",
                    BuildWarningLevel.W3 => "/W3",
                    _ => throw new InvalidBuildWarningLevelException("Invalid compiler warning level")
                }}"
            ]);

            if (!BuildContext.EnableExceptions)
                Defines.Add("_HAS_EXCEPTIONS=0");

            LibrarianOptions.Add("/nologo");
            
            CompilerOptions.AddRange([
                "/nologo", 
                "/showIncludes", 
                "/utf-8", 
                "/GS", 
                "/Zi"
            ]);
            
            LinkerOptions.AddRange([
                "/nologo", 
                "/DEBUG", 
                $"/MACHINE:{(BuildArchitecture == TargetArchitecture.x64? "X64" : "ARM64")}"
            ]);

            switch (BuildContext.CurrentBuildConfig)
            {
                case TargetConfiguration.Debug:
                    CompilerOptions.AddRange(["/MDd", "/Od", "/Oy-", "/RTC1"]);
                    LinkerOptions.AddRange(["/INCREMENTAL:NO"]);
                    break;
                case TargetConfiguration.Develop:
                    CompilerOptions.AddRange(["/MDd", "/O1", "/Oy-"]);
                    break;
                case TargetConfiguration.Release:
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
            var res = Utils.RunProcess(Path.Join(CompilerBinDirectory, CompilerName), $"/nologo /c /std:{BuildContext.CurrentCxxVersion} {String.Join(" ", IncludeDirs.Select(dir => $"/I\"{dir}\""))}  \"{filePath}\" /Fo\"{objPath}\"");
            return res.ExitCode == 0;
        }
    }
}
