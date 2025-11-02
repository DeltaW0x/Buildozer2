using ClangSharp;
using ClangSharp.Interop;
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
        
        /*
        var assemblies = ScriptCompiler.CompileAssemblies([@"C:\Users\lucac\Desktop\build.cs"]);
        var baseType = typeof(TestClass);
        foreach(var assembly in assemblies)
        {
            var scriptType = assembly.GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));

            if (scriptType == null)
            {
                Log.Warning($"No class implementing BaseClass found in script, skipping");
                continue;
            }

            TestClass? instance = Activator.CreateInstance(scriptType) as TestClass;
            instance!.Execute();
        }
        */
    }
}

