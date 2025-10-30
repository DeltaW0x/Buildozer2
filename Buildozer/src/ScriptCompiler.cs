using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Buildozer;

public static class ScriptCompiler
{
    public static Assembly CompileAssembly(string path, string cachePath = ".cache")
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();
        
        string cachedPath = Path.Combine(cachePath, Utils.HashPath(path) + ".dll");
        if (File.Exists(cachedPath))
        {
            Console.WriteLine($"Using cached assembly at {cachedPath}");
            return Assembly.LoadFrom(cachedPath);
        }

        Console.WriteLine("Couldn't find cached assembly, compiling assembly from scratch");

        string code = File.ReadAllText(path);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithAllowUnsafe(true)
            .WithOptimizationLevel(OptimizationLevel.Debug)
            .WithNullableContextOptions(NullableContextOptions.Enable);
        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetFileNameWithoutExtension(path),
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: options
        );

        using MemoryStream ms = new();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success)
        {
            string errors = string.Join("\n", result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            throw new Exception("Compilation failed:\n" + errors);
        }

        ms.Seek(0, SeekOrigin.Begin);
        Assembly assembly = Assembly.Load(ms.ToArray());

        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
        
        using FileStream outFile = File.Create(cachedPath);
        Console.WriteLine($"Saving cached assembly to {cachedPath}");
        ms.CopyTo(outFile);

        return assembly;
    }
}