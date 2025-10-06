using Buildozer.BuildTool;
namespace Buildozer.CLI;

class Program
{
    public static void Main(string[] args)
    {
        var toolchains = Toolchain.DiscoverSystemToolchains();
        /*
        foreach (var toolchain in toolchains) {
            Console.WriteLine($"Name: {toolchain.Name}");
            Console.WriteLine($"Version: {toolchain.Version}");
            Console.WriteLine($"CrossCompiler: {toolchain.IsCrossCompiler}");
            Console.WriteLine();
        }
        */
    }
}

