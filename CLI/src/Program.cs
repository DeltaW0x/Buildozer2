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
        File.WriteAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaToolchain());
        File.AppendAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaCxxCompilationCommand("main.cpp"));
        File.AppendAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaLinkCommand(false, "main", ["main.obj"]));
    }
}

