using Buildozer.BuildTool;

namespace Buildozer.CLI;

class Program
{
    public static void Main(string[] args)
    {

        Toolchain[] toolchains = Toolchain.DiscoverSystemToolchains();

        foreach(var toolchain in toolchains)
        {
            if(toolchain is MsvcToolchain)
            {
                var msvcToolchain = toolchain as MsvcToolchain;

                Console.WriteLine($"Name: {msvcToolchain!.Name}");
                Console.WriteLine($"Windows SDK path: {msvcToolchain!.WinSdkRoot}");
                Console.WriteLine($"Windows SDK version: {msvcToolchain!.WinSdkVersion}");
                
                Console.WriteLine($"Visual studio version: {msvcToolchain!.VsVersion}");

                Console.WriteLine($"MSVC path: {msvcToolchain.CompilerRoot}");
                Console.WriteLine($"MSVC version: {msvcToolchain.CompilerVersion}");
                
                Console.WriteLine($"Crosscompiler: {msvcToolchain.IsCrossCompiler}");

                Console.WriteLine();
            }
            
            else if(toolchain is ClangToolchain)
            {
                var clangToolchain = toolchain as ClangToolchain;

                Console.WriteLine($"Name: {clangToolchain!.Name}");

                Console.WriteLine($"Clang path: {clangToolchain.CompilerRoot}");
                Console.WriteLine($"Clang version: {clangToolchain.CompilerVersion}");
                
                Console.WriteLine($"Crosscompiler: {clangToolchain.IsCrossCompiler}");

                Console.WriteLine();
            }
        }
    }
}

