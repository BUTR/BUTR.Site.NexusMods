using BUTR.Site.NexusMods.Server.Extensions;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Utils.Reflection;

internal static class CSharpILMixedLanguage
{
    public static ReflectionDisassembler CreateDisassembler(ITextOutput output, CancellationToken ct)
    {
        var methodBodyDisassembler = new MixedMethodBodyDisassembler(output, ct)
        {
            ShowMetadataTokens = false,
            ShowMetadataTokensInBase10 = false,
            ShowRawRVAOffsetAndBytes = false,
            ShowSequencePoints = false,
            DetectControlStructure = true,
        };

        return new(output, methodBodyDisassembler, ct)
        {
            ShowMetadataTokens = false,
            ShowMetadataTokensInBase10 = false,
            ShowRawRVAOffsetAndBytes = false,
            ShowSequencePoints = false,
            DetectControlStructure = true,
            ExpandMemberDefinitions = false,
        };
    }

    private static CSharpDecompiler CreateDecompiler(PEFile module, DecompilerSettings settings, CancellationToken ct)
    {
        var resolver = new UniversalAssemblyResolver(module.Name, false, module.DetectTargetFrameworkId(), module.DetectRuntimePack(), PEStreamOptions.Default, MetadataReaderOptions.None);
        return new CSharpDecompiler(module, resolver, settings) { CancellationToken = ct };
    }

    private static void WriteCode(TextWriter output, DecompilerSettings settings, SyntaxTree syntaxTree)
    {
        syntaxTree.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true });
        TokenWriter tokenWriter = new TextWriterTokenWriter(output) { IndentationString = settings.CSharpFormattingOptions.IndentationString };
        tokenWriter = TokenWriter.WrapInWriterThatSetsLocationsInAST(tokenWriter);
        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, settings.CSharpFormattingOptions));
    }

    private class MixedMethodBodyDisassembler : MethodBodyDisassembler
    {
        private static readonly char[] NewLine = Environment.NewLine.ToArray();
        private static readonly List<ICSharpCode.Decompiler.DebugInfo.SequencePoint> Empty = new();

        // list sorted by IL offset
        private Dictionary<int, ICSharpCode.Decompiler.DebugInfo.SequencePoint>? sequencePoints;

        // lines of raw c# source code
        private StringBuilder? _stringBuilder;
        private List<(int, int)>? _stringBuilderLinesIndices;

        private readonly CancellationToken _cancellationToken;

        public MixedMethodBodyDisassembler(ITextOutput output, CancellationToken ct) : base(output, ct)
        {
            _cancellationToken = ct;
        }

        public override void Disassemble(PEFile module, MethodDefinitionHandle handle)
        {
            try
            {
                var settings = new DecompilerSettings(LanguageVersion.Latest);
                var decompiler = CreateDecompiler(module, settings, _cancellationToken);
                var syntaxTree = decompiler.Decompile(handle);
                
                using var csharpOutput = new StringWriter();
                WriteCode(csharpOutput, settings, syntaxTree);
                var mapping = decompiler.CreateSequencePoints(syntaxTree).FirstOrDefault(kvp => (kvp.Key.MoveNextMethod ?? kvp.Key.Method)?.MetadataToken == handle);
                sequencePoints = (mapping.Value ?? Empty).ToDictionary(x => x.Offset, x => x);
                
                _stringBuilder = csharpOutput.GetStringBuilder();
                IndexStringBuilder();
                
                base.Disassemble(module, handle);
            }
            finally
            {
                sequencePoints = null;
                _stringBuilder = null;
                _stringBuilderLinesIndices = null;
            }
        }

        private void IndexStringBuilder()
        {
            if (_stringBuilder is null) return;

            _stringBuilderLinesIndices = new List<(int, int)>();
            var previousIdx = 0;
            while (_stringBuilder.IndexOf(NewLine, previousIdx) is var idx and not -1)
            {
                _stringBuilderLinesIndices.Add((previousIdx, idx));
                previousIdx = idx + NewLine.Length;
            }

            _stringBuilderLinesIndices.Add((previousIdx, _stringBuilder.Length));
        }

        // Is called within base.Disassemble
        protected override void WriteInstruction(ITextOutput output, MetadataReader metadata, MethodDefinitionHandle methodHandle, ref BlobReader blob, int methodRva)
        {
            if (output is not PlainTextOutput2 plainTextOutput2) return;
            if (_stringBuilder is null || _stringBuilderLinesIndices is null) return;
            if (sequencePoints is null) return;

            if (sequencePoints.TryGetValue(blob.Offset, out var info))
            {
                if (info.IsHidden)
                {
                    plainTextOutput2.WriteLine("// (no C# code)");
                }
                else
                {
                    for (var line = info.StartLine; line <= info.EndLine; line++)
                    {
                        //if (_stringBuilderLinesIndices.Count < line) continue; // TODO:

                        plainTextOutput2.WriteLine();
                        plainTextOutput2.Write("// ");

                        var (start, end) = _stringBuilderLinesIndices[line - 1];
                        var length = end - start;
                        plainTextOutput2.Write(_stringBuilder, start, length);
                        plainTextOutput2.WriteLine();
                    }
                }
            }

            base.WriteInstruction(plainTextOutput2, metadata, methodHandle, ref blob, methodRva);
        }
    }
}