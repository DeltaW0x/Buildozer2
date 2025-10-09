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
                Console.WriteLine($"Name: {toolchain.Name}, MSVC Version {toolchain.CompilerVersion}, Visual Studio version: {((MsvcToolchain)toolchain).VsVersion}");
                Console.WriteLine();
            }
        }
    }
}

