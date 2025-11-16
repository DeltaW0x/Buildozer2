using Buildozer.BuildTool;
using ClangSharp;
using Serilog;
using Spectre.Console;
using System.Globalization;
using System.Text.Json;
namespace Buildozer.CLI;

partial class Program
{
    static void Main()
    {
        /*
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        Toolchain[] toolchains = Toolchain.DiscoverSystemToolchains();
        File.WriteAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaToolchain());
        File.AppendAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaCxxCompilationCommand(@"C:/Users/lucac/Desktop/compilation_test/src/main.cpp"));
        File.AppendAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaLinkCommand(false, @"C:/Users/lucac/Desktop/compilation_test/main", [@"C:/Users/lucac/Desktop/compilation_test/builddir/src/main.obj"]));
        */
    }
}

