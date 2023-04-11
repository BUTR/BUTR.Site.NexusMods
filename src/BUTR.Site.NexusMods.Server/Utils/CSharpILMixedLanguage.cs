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
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Utils
{
    internal static class CSharpILMixedLanguage
    {
        public static ReflectionDisassembler CreateDisassembler(ITextOutput output) =>
            new(output, new MixedMethodBodyDisassembler(output) { DetectControlStructure = true, ShowSequencePoints = false }, CancellationToken.None)
            {
                ShowMetadataTokens = false,
                ShowMetadataTokensInBase10 = false,
                ShowRawRVAOffsetAndBytes = false,
                ExpandMemberDefinitions = false
            };

        private static CSharpDecompiler CreateDecompiler(PEFile module)
        {
            var resolver = new UniversalAssemblyResolver(module.Name, false, module.DetectTargetFrameworkId());
            var decompiler = new CSharpDecompiler(module, resolver, new DecompilerSettings()) { CancellationToken = CancellationToken.None };
            return decompiler;
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

            public MixedMethodBodyDisassembler(ITextOutput output) : base(output, CancellationToken.None) { }

            public override void Disassemble(PEFile module, MethodDefinitionHandle handle)
            {
                try
                {
                    var csharpOutput = new StringWriter();
                    var decompiler = CreateDecompiler(module);
                    var st = decompiler.Decompile(handle);
                    WriteCode(csharpOutput, new DecompilerSettings(), st);
                    var mapping = decompiler.CreateSequencePoints(st).FirstOrDefault(kvp => (kvp.Key.MoveNextMethod ?? kvp.Key.Method)?.MetadataToken == handle);
                    sequencePoints = mapping.Value ?? (IList<ICSharpCode.Decompiler.DebugInfo.SequencePoint>) EmptyList<ICSharpCode.Decompiler.DebugInfo.SequencePoint>.Instance;
                    codeLines = csharpOutput.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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
}