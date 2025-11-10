using Buildozer.BuildTool;
using Serilog;
namespace Buildozer.CLI;

partial class Program
{
    static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        Toolchain[] toolchains = Toolchain.DiscoverSystemToolchains();
        Console.WriteLine(toolchains[0].GenerateNinjaToolchain());
    }
}

