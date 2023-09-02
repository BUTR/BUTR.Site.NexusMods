using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace BUTR.Site.NexusMods.Server.Utils;

public static class ScriptHandler
{
    private class UnloadableAssemblyLoadContext : AssemblyLoadContext
    {
        public UnloadableAssemblyLoadContext() : base(true) { }
    }

    static ScriptHandler()
    {
        static void LoadReferencedAssembly(Assembly assembly)
        {
            foreach (var name in assembly.GetReferencedAssemblies())
            {
                if (AppDomain.CurrentDomain.GetAssemblies().All(a => a.FullName != name.FullName))
                {
                    LoadReferencedAssembly(Assembly.Load(name));
                }
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            LoadReferencedAssembly(assembly);
    }

    public static void CompileAndExecute(string filename, string sourceCode)
    {
        static void LoadAndExecute(Stream stream)
        {
            var assemblyLoadContext = new UnloadableAssemblyLoadContext();

            var assembly = assemblyLoadContext.LoadFromStream(stream);

            assembly.GetType("Script")?.GetMethod("Run")?.Invoke(null, null);

            assemblyLoadContext.Unload();
        }

        var codeString = SourceText.From(sourceCode);
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic)
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create($"{filename}.dll",
            new[] { parsedSyntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success) return;
        ms.Seek(0, SeekOrigin.Begin);
        LoadAndExecute(ms);
    }
}