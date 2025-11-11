using Buildozer.BuildTool;
using Buildozer.ShaderCompiler;
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
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        /*
        Toolchain[] toolchains = Toolchain.DiscoverSystemToolchains();
        File.WriteAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaToolchain());
        File.AppendAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaCxxCompilationCommand("main.cpp"));
        File.AppendAllText(@"C:\Users\lucac\Desktop\compilation_test\build.ninja", toolchains[0].GenerateNinjaLinkCommand(false, "main", ["main.obj"]));

        BuildContext.CurrentBuildConfig = BuildConfig.Debug;
        Toolchain debugToolchain = Toolchain.DiscoverSystemToolchains()[0];

        BuildContext.CurrentBuildConfig = BuildConfig.Develop;
        Toolchain developToolchain = Toolchain.DiscoverSystemToolchains()[0];

        BuildContext.CurrentBuildConfig = BuildConfig.Release;
        Toolchain releaseToolchain = Toolchain.DiscoverSystemToolchains()[0];

        Console.WriteLine(JsonSerializer.Serialize(debugToolchain, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
        Console.WriteLine(JsonSerializer.Serialize(developToolchain, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
        Console.WriteLine(JsonSerializer.Serialize(releaseToolchain, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
        */

        Scanner scanner = new Scanner("(10 * 2) - 3");
        Parser parser = new Parser(scanner.ScanTokens());

        Console.WriteLine(new GLSLBackend().Compile(parser.Parse()));
    }
}

