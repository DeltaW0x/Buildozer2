using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Serilog;
using System.Reflection;

namespace Buildozer.BuildTool;

public static class ScriptCompiler
{
    public static Assembly[] CompileAssemblies(string[] paths, string cachePath = ".cache")
    {
        List<Assembly> assemblies = new();

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        foreach (var path in paths)
        {
            string cachedFilePath = Path.Combine(cachePath, Utils.HashPath(path) + ".dll");

            if (File.Exists(cachedFilePath))
            {
                Log.Information($"Using cached assembly at {Path.GetFileName(cachedFilePath)} for file {Path.GetFileName(path)}");
                assemblies.Add(Assembly.LoadFrom(cachedFilePath));
                continue;
            }

            Log.Information($"Couldn't find cached assembly for {Path.GetFileName(path)}, compiling from scratch");

            string code = File.ReadAllText(path);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithNullableContextOptions(NullableContextOptions.Enable);
            var compilation = CSharpCompilation.Create(
                assemblyName: Path.GetFileNameWithoutExtension(path),
                syntaxTrees: [syntaxTree],
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
                Log.Error($"Compilation of {Path.GetFileName(path)} failed:{errors}");
                continue;
            }

            ms.Seek(0, SeekOrigin.Begin);
            Directory.CreateDirectory(Path.GetDirectoryName(cachedFilePath)!);

            using FileStream outFile = File.Create(cachedFilePath);
            Log.Information($"Saving cached assembly to {Path.GetFileName(cachedFilePath)}");
            ms.CopyTo(outFile);

            assemblies.Add(Assembly.Load(ms.ToArray()));
        }
        return assemblies.ToArray();
    }
}

