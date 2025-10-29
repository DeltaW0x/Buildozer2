using Spectre.Console;
using System.Diagnostics;
namespace Buildozer.CLI;

partial class Program
{
    static void Main()
    {
        Console.WriteLine($"Does explorer exist? {Utils.CheckProgram("explorer", out string? path)} { path ?? "" }");
    }
}

