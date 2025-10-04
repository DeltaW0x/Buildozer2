using Buildozer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;

namespace CLI
{
    internal class Program
    {

        public static List<BuildTask> LoadBuildScripts(string path)
        {
            var tasks = new List<BuildTask>();

            var files = Directory.GetFiles(path, "*.build.cs");

            if (!files.Any())
                return tasks;

            // Parse all script files into syntax trees
            var syntaxTrees = files.Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)));

            // Get references: include the current assembly and all its dependencies
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            // Compile them into a single in-memory assembly
            var compilation = CSharpCompilation.Create(
                assemblyName: "RuntimeScripts_" + Guid.NewGuid(),
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var errors = string.Join(Environment.NewLine, result.Diagnostics);
                throw new Exception("Script compilation failed:\n" + errors);
            }

            ms.Seek(0, SeekOrigin.Begin);
            var asm = Assembly.Load(ms.ToArray());

            // Find all types that inherit from BuildTask
            var taskTypes = asm.GetTypes().Where(t => typeof(BuildTask).IsAssignableFrom(t) && !t.IsAbstract);

            // Instantiate each one
            foreach (var type in taskTypes)
            {
                var instance = (BuildTask)Activator.CreateInstance(type)!;
                tasks.Add(instance);
            }

            return tasks;
        }

        static void Main(string[] args)
        {
            var tasks = LoadBuildScripts(@"C:\\Users\\lucac\\Desktop\\test");

            foreach (var task in tasks)
            {
                Console.WriteLine(task.Run());
            }
        }
    }
}
