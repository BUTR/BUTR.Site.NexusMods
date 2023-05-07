using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Shared.Helpers;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Utils;

public record RecreatedStacktrace(string Method, string CSharpWithIL, int LineNumber);

public static class RecreateStacktraceUtils
{
    private static unsafe Span<byte> GetReaderSpan(in BlobReader reader) => new(reader.CurrentPointer, reader.Length);

    public static IEnumerable<RecreatedStacktrace> GetRecreatedStacktrace(IEnumerable<string> assemblyFiles, CrashReport crashReport, CancellationToken ct)
    {
        static int GetCodeLineNumber(string code, int ilOffset)
        {
            var lineNumber = 1;
            var toSearch = $"IL_{ilOffset:x4}:";
            foreach (ReadOnlySpan<char> line in code.SplitLines())
            {
                if (line.IndexOf(toSearch) != -1)
                    return lineNumber;
                lineNumber++;
            }
            return -1;
        }
        
        static IEnumerable<PEFile> GetPEFiles(IEnumerable<string> assemblyFiles)
        {
            foreach (var assemblyFile in assemblyFiles)
            {
                PEFile moduleDefinition;
                try { moduleDefinition = new PEFile(assemblyFile, PEStreamOptions.Default, MetadataReaderOptions.None); }
                catch (Exception) { continue; }
                yield return moduleDefinition;
            }
        }

        static IEnumerable<RecreatedStacktrace> RecreatedStacktrace(PEFile moduleDefinition, CrashReport crashReport, CancellationToken ct)
        {
            const int dotLength = 1;
            
            using var _ = moduleDefinition;
            foreach (var frame in crashReport.EnhancedStacktrace.SelectMany(y => y.Methods))
            {
                var utf8MethodName = Encoding.UTF8.GetBytes(frame.Method);
                var foundMethodHandle = moduleDefinition.Metadata.TypeDefinitions.Select(moduleDefinition.Metadata.GetTypeDefinition).Where(x =>
                {
                    var methodNameSpan = utf8MethodName.AsSpan();

                    var typeNamespaceReader = moduleDefinition.Metadata.GetBlobReader(x.Namespace);
                    var typeNameReader = moduleDefinition.Metadata.GetBlobReader(x.Name);
                    if (methodNameSpan.Length < typeNamespaceReader.Length + dotLength + typeNameReader.Length + dotLength) return false;

                    if (!methodNameSpan.Slice(0, typeNamespaceReader.Length).SequenceEqual(GetReaderSpan(typeNamespaceReader))) return false;

                    return methodNameSpan.Slice(typeNamespaceReader.Length + dotLength, typeNameReader.Length).SequenceEqual(GetReaderSpan(typeNameReader));
                }).Select(x => (Type: x, Methods: x.GetMethods())).Select(x =>
                {
                    var type = x.Type;

                    var typeNamespaceReader = moduleDefinition.Metadata.GetBlobReader(type.Namespace);
                    var typeNameReader = moduleDefinition.Metadata.GetBlobReader(type.Name);

                    return x.Methods.Where(y =>
                    {
                        var methodNameSpan = utf8MethodName.AsSpan();

                        var method = moduleDefinition.Metadata.GetMethodDefinition(y);

                        var methodNameReader = moduleDefinition.Metadata.GetBlobReader(method.Name);
                        if (methodNameSpan.Length != typeNamespaceReader.Length + dotLength + typeNameReader.Length + dotLength + methodNameReader.Length) return false;

                        return methodNameSpan.Slice(typeNamespaceReader.Length + dotLength + typeNameReader.Length + dotLength, methodNameReader.Length).SequenceEqual(GetReaderSpan(methodNameReader));
                    });
                }).SelectMany(x => x).FirstOrDefault();
                if (foundMethodHandle.Equals(default)) continue;

                // The section above takes about 1ms and 2MB
                // The section below takes about 23ms and 150MB
                var output = new PlainTextOutput();
                var disassembler = CSharpILMixedLanguage.CreateDisassembler(output, ct);
                disassembler.DisassembleMethod(moduleDefinition, foundMethodHandle);

                var code = output.ToString();
                var ilOffset = crashReport.EnhancedStacktrace.First(x => x.Methods.Any(y => y == frame)).ILOffset;
                var lineNumber = GetCodeLineNumber(code, ilOffset);

                yield return new RecreatedStacktrace(frame.Method, code, lineNumber);
            }
        }

        var methods = crashReport.EnhancedStacktrace.SelectMany(y => y.Methods).ToArray();
        return GetPEFiles(assemblyFiles).AsParallel().AsUnordered().WithCancellation(ct)
            .SelectMany(moduleDefinition => RecreatedStacktrace(moduleDefinition, crashReport, ct))
            .OrderBy(x => Array.FindIndex(methods, y => y.Method == x.Method));
    }
}