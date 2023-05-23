using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Utils.Reflection;
using BUTR.Site.NexusMods.Shared.Extensions;
using BUTR.Site.NexusMods.Shared.Helpers;

using ICSharpCode.Decompiler.Metadata;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Utils;

public record RecreatedStacktrace(string Method, string CSharpWithIL, int LineNumber);

public static class RecreateStacktraceUtils
{
    public static IEnumerable<RecreatedStacktrace> GetRecreatedStacktrace(IEnumerable<string> assemblyFiles, CrashReport crashReport, CancellationToken ct)
    {
        var methods = crashReport.EnhancedStacktrace.SelectMany(y => y.Methods).ToArray();
        return GetRecreatedStacktraceUnordered(assemblyFiles, crashReport, ct)
            .OrderBy(x => Array.FindIndex(methods, y => y.Method == x.Method));

    }
    public static IEnumerable<RecreatedStacktrace> GetRecreatedStacktraceUnordered(IEnumerable<string> assemblyFiles, CrashReport crashReport, CancellationToken ct)
    {
        // The Harmony Like Full Description
        static string FullDescription(PEFile moduleDefinition, MethodDefinitionHandle methodHandle, MethodDefinition method)
        {
            if (methodHandle.IsNil) return "null";

            var result = new StringBuilder();
            var methodSignature = method.DecodeSignature(new DisassemblingTypeProvider(), new MetadataGenericContext(methodHandle, moduleDefinition));
            if (!methodSignature.Header.Attributes.HasFlag(SignatureAttributes.Instance))
                result.Append("static ");
            if (method.Attributes.HasFlag(MethodAttributes.Abstract))
                result.Append("abstract ");
            if (method.Attributes.HasFlag(MethodAttributes.Virtual))
                result.Append("virtual ");
            result.Append(methodSignature.ReturnType).Append(' ');
            if (method.GetDeclaringType() is { IsNil: false } declaringTypeHandle && moduleDefinition.Metadata.GetTypeDefinition(declaringTypeHandle) is var type)
                result.Append(moduleDefinition.Metadata.GetStringRawSpan(type.Namespace)).Append('.').Append(moduleDefinition.Metadata.GetStringRawSpan(type.Name)).Append('.');
            result.Append(moduleDefinition.Metadata.GetStringRawSpan(method.Name)).Append('(').AppendJoin(", ", methodSignature.ParameterTypes.Select(x => x.ToString())).Append(')');
            return result.ToString();
        }

        // Allocates string memory, do not use often
        static string GetMethodNameWithoutGenericTypeParametersAndParameterNames(string methodFullName)
        {
            static string RemoveGenerics(string methodFullName)
            {
                var span = methodFullName.AsSpan();

                var sb = new StringBuilder();
                while (span.IndexOf('<') is var idxStart and not -1 && span.Slice(idxStart).IndexOf('>') is var idxEnd and not -1)
                {
                    sb.Append(span.Slice(0, idxStart));
                    span = span.Slice(idxStart + idxEnd + 1);
                }

                sb.Append(span);

                return sb.ToString();
            }

            var span = RemoveGenerics(methodFullName).AsSpan();

            var sb = new StringBuilder();

            if (span.IndexOf('(') is var idxStart and not -1)
            {
                sb.Append(span.Slice(0, idxStart + 1));
                span = span.Slice(idxStart + 1);

                while (span.IndexOf(", ") is var idx2Start and not -1 && span.Slice(0, idx2Start).IndexOf(' ') is var spaceIdx)
                {
                    sb.Append(span.Slice(0, spaceIdx)).Append(", ");
                    span = span.Slice(idx2Start + 2);
                }

                if (span.IndexOf(' ') is var spaceIdx2 and not -1)
                {
                    sb.Append(span.Slice(0, spaceIdx2));
                    var idxEnd = span.IndexOf(')');
                    span = span.Slice(idxEnd);
                }
            }

            sb.Append(span);

            return sb.ToString();
        }

        static int GetLineNumber(string code, int ilOffset)
        {
            if (ilOffset == -1)
                return -1;

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
                var methodName = GetMethodNameWithoutGenericTypeParametersAndParameterNames(frame.Method);
                var methodFullName = GetMethodNameWithoutGenericTypeParametersAndParameterNames(frame.MethodFullName);

                var utf8MethodName = Encoding.UTF8.GetBytes(methodName);
                var foundMethods = moduleDefinition.Metadata.TypeDefinitions.Select(moduleDefinition.Metadata.GetTypeDefinition).Where(x =>
                {
                    // Filter Type Namespace and Name
                    var methodNameSpan = utf8MethodName.AsSpan();

                    var typeNamespaceReader = moduleDefinition.Metadata.GetBlobReader(x.Namespace);
                    var typeNameReader = moduleDefinition.Metadata.GetBlobReader(x.Name);

                    if (methodNameSpan.Length < typeNamespaceReader.Length + dotLength + typeNameReader.Length + dotLength) return false;

                    var methodNameSlice = methodNameSpan.Slice(0, typeNamespaceReader.Length);
                    if (!methodNameSlice.SequenceEqual(typeNamespaceReader.GetStringRawSpan())) return false;

                    methodNameSlice = methodNameSpan.Slice(typeNamespaceReader.Length + dotLength, typeNameReader.Length);
                    if (!methodNameSlice.SequenceEqual(typeNameReader.GetStringRawSpan())) return false;
                    return true;
                }).Select(x => (Type: x, Methods: x.GetMethods())).Select(x =>
                {
                    // Filter Method Name
                    var type = x.Type;

                    var typeNamespaceReader = moduleDefinition.Metadata.GetBlobReader(type.Namespace);
                    var typeNameReader = moduleDefinition.Metadata.GetBlobReader(type.Name);

                    return x.Methods.Select(y => (Type: x.Type, MethodHandle: y, Method: moduleDefinition.Metadata.GetMethodDefinition(y))).Where(y =>
                    {
                        var methodNameSpan = utf8MethodName.AsSpan();

                        var methodNameReader = moduleDefinition.Metadata.GetBlobReader(y.Method.Name);

                        if (methodNameSpan.Length != typeNamespaceReader.Length + dotLength + typeNameReader.Length + dotLength + methodNameReader.Length)
                            return false;

                        var methodNameSlice = methodNameSpan.Slice(typeNamespaceReader.Length + dotLength + typeNameReader.Length + dotLength, methodNameReader.Length);
                        return methodNameSlice.SequenceEqual(methodNameReader.GetStringRawSpan());
                    });
                }).SelectMany(x => x).ToImmutableArray(); // Get all possibilities
                MethodDefinitionHandle foundMethodHandle;
                if (foundMethods.Length == 0)
                {
                    foundMethodHandle = default;
                }
                else if (foundMethods.Length == 1)
                {
                    foundMethodHandle = foundMethods[0].MethodHandle;
                }
                else
                {
                    foundMethodHandle = foundMethods.Where(x =>
                    {
                        // Filter Method Parameters
                        var type = x.Type;
                        var methodHandle = x.MethodHandle;
                        var method = x.Method;

                        // We try to compare full method signatures, doesn't work for generics.
                        var fullName = FullDescription(moduleDefinition, methodHandle, method);
                        return methodFullName == fullName;
                    }).Select(x => x.MethodHandle).FirstOrDefault();
                    if (foundMethodHandle.IsNil)
                    {
                        // If the comparison didn't work, try to at least use parameter count
                        var parameterDelimiterCount = methodFullName.Count(x => x == ',');
                        var parameterCount = parameterDelimiterCount == 0 ? 0 : parameterDelimiterCount + 1;
                        foundMethodHandle = foundMethods.Where(x => x.Method.GetGenericParameters().Count == parameterCount).Select(x => x.MethodHandle).FirstOrDefault();
                    }
                }

                if (foundMethodHandle.IsNil) continue;

                // The section above takes about 1ms and 2MB
                // The section below takes about 23ms and 150MB
                var output = new PlainTextOutput2();
                var disassembler = CSharpILMixedLanguage.CreateDisassembler(output, ct);
                disassembler.DisassembleMethod(moduleDefinition, foundMethodHandle);

                var code = output.ToString()!;
                var ilOffset = crashReport.EnhancedStacktrace.First(x => x.Methods.Any(y => y == frame)).ILOffset;
                var lineNumber = GetLineNumber(code, ilOffset);

                yield return new RecreatedStacktrace(frame.Method, code, lineNumber);
            }
        }

        return GetPEFiles(assemblyFiles).AsParallel().AsUnordered().WithCancellation(ct)
            .SelectMany(moduleDefinition => RecreatedStacktrace(moduleDefinition, crashReport, ct));
    }
}