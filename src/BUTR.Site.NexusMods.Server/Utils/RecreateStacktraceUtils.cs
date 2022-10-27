using BUTR.Site.NexusMods.Shared.Helpers;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

namespace BUTR.Site.NexusMods.Server.Utils
{
    public record RecreatedStacktrace(string Method, string CSharpWithIL, int InstructionIndex);

    public static class RecreateStacktraceUtils
    {
        public static IEnumerable<RecreatedStacktrace> GetRecreatedStacktrace(IEnumerable<string> binaryFiles, CrashReport crashReport)
        {
            const BindingFlags all = BindingFlags.Public |
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance |
                                     BindingFlags.Static |
                                     BindingFlags.GetField |
                                     BindingFlags.SetField |
                                     BindingFlags.GetProperty |
                                     BindingFlags.SetProperty;

            var alc = new AssemblyLoadContext(crashReport.GameVersion, true);
            foreach (var binaryFile in binaryFiles)
            {
                try
                {
                    alc.LoadFromAssemblyPath(binaryFile);
                }
                catch (Exception) { }
            }

            var methodWithFoundMethodInfo = crashReport.EnhancedStacktrace.SelectMany(x => x.Methods).Select(x =>
            {
                var found = alc.Assemblies.SelectMany(y =>
                {
                    Type?[] types;
                    try
                    {
                        types = y.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types = e.Types;
                    }
                    return types.SelectMany(z => z?.GetMethods(all) ?? Array.Empty<MethodInfo>());
                }).FirstOrDefault(y => $"{y.DeclaringType?.FullName}.{y.Name}" == x.Method);
                return (x.Method, found);
            }).ToArray();

            foreach (var (method, methodInfo) in methodWithFoundMethodInfo)
            {
                if (methodInfo is null || methodInfo.DeclaringType is null) continue;

                var ilOffset = crashReport.EnhancedStacktrace.First(x => x.Methods.Any(y => y.Method == method)).ILOffset;

                var path = methodInfo.DeclaringType.Assembly.Location;
                var moduleDefinition = new PEFile(path);
                var resolver = new UniversalAssemblyResolver(Path.GetFileNameWithoutExtension(path), false, moduleDefinition.DetectTargetFrameworkId(), null, PEStreamOptions.PrefetchEntireImage);
                var decompiler = new CSharpDecompiler(moduleDefinition, resolver, new DecompilerSettings());

                var foundType = decompiler.TypeSystem.MainModule.TypeDefinitions.FirstOrDefault(x => x.Namespace == methodInfo.DeclaringType.Namespace && x.Name == methodInfo.DeclaringType.Name);
                if (foundType is null) continue;
                var foundMethod = foundType.Methods.FirstOrDefault(x => x.Name == methodInfo.Name);
                if (foundMethod is null) continue;

                var output = new PlainTextOutput();
                var disassembler = CSharpILMixedLanguage.CreateDisassembler(output);
                disassembler.DisassembleMethod(moduleDefinition, (MethodDefinitionHandle) foundMethod.MetadataToken);
                var code = output.ToString();
                var idx = code.IndexOf($"IL_{ilOffset:x4}:", StringComparison.Ordinal);

                yield return new RecreatedStacktrace(method, code, idx);
            }

            alc.Unload();
        }

    }
}