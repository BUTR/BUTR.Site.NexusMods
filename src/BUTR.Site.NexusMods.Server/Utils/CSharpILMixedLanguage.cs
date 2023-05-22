using BUTR.Site.NexusMods.Server.Extensions;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.Util;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Utils;

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
        // list sorted by IL offset
        private IList<ICSharpCode.Decompiler.DebugInfo.SequencePoint>? sequencePoints;

        // lines of raw c# source code
        private string[]? codeLines;

        private readonly CancellationToken cancellationToken;

        public MixedMethodBodyDisassembler(ITextOutput output, CancellationToken ct) : base(output, ct)
        {
            cancellationToken = ct;
        }

        public override void Disassemble(PEFile module, MethodDefinitionHandle handle)
        {
            try
            {
                var settings = new DecompilerSettings(LanguageVersion.Latest);
                using var csharpOutput = new StringWriter();
                var decompiler = CreateDecompiler(module, settings, cancellationToken);
                var st = decompiler.Decompile(handle);
                WriteCode(csharpOutput, settings, st);
                var mapping = decompiler.CreateSequencePoints(st).FirstOrDefault(kvp => (kvp.Key.MoveNextMethod ?? kvp.Key.Method)?.MetadataToken == handle);
                sequencePoints = mapping.Value ?? (IList<ICSharpCode.Decompiler.DebugInfo.SequencePoint>) EmptyList<ICSharpCode.Decompiler.DebugInfo.SequencePoint>.Instance;
                codeLines = csharpOutput.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None); // TODO:
                base.Disassemble(module, handle);
            }
            finally
            {
                sequencePoints = null;
                codeLines = null;
            }
        }

        protected override void WriteInstruction(ITextOutput output, MetadataReader metadata, MethodDefinitionHandle methodHandle, ref BlobReader blob, int methodRva)
        {
            if (codeLines is not null && sequencePoints?.BinarySearch(blob.Offset, seq => seq.Offset) is { } index and >= 0)
            {
                var info = sequencePoints[index];
                if (!info.IsHidden)
                {
                    for (var line = info.StartLine; line <= info.EndLine; line++)
                    {
                        output.WriteLine();
                        output.WriteLine($"// {codeLines[line - 1].Trim()}");
                    }
                }
                else
                {
                    output.WriteLine("// (no C# code)");
                }
            }

            base.WriteInstruction(output, metadata, methodHandle, ref blob, methodRva);
        }
    }
}