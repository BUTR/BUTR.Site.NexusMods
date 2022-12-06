using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BUTR.Site.NexusMods.Client.Utils
{
    public record LocalizationEntry(string Id, string Original);

    public record StringOwner(string Text, string Owner);

    public static class AssemblyReader
    {
        public static IEnumerable<LocalizationEntry> ParseAssemblyLocalizations(Stream assemblyStream)
        {
            foreach (var str in ParseAssemblyStrings(assemblyStream))
                if (LocalizationUtils.TryParseTranslationString(str.Text, out var id, out var content))
                    yield return new(id, content);
        }

        public static IEnumerable<StringOwner> ParseAssemblyNonLocalizations(Stream assemblyStream)
        {
            foreach (var str in ParseAssemblyStrings(assemblyStream))
                if (!LocalizationUtils.TryParseTranslationString(str.Text, out _, out _))
                    yield return str;
        }

        public static IEnumerable<StringOwner> ParseAssemblyStrings(Stream assemblyStream)
        {
            using var reader = new PEReader(assemblyStream);
            var metadata = reader.GetMetadataReader();

            foreach (var fieldDefinition in metadata.FieldDefinitions.Select(metadata.GetFieldDefinition))
            {
                if (!fieldDefinition.Attributes.HasFlag(FieldAttributes.Literal))
                    continue;

                var constantHandle = fieldDefinition.GetDefaultValue();
                if (constantHandle.IsNil)
                    continue;

                var constant = metadata.GetConstant(constantHandle);
                if (constant.TypeCode != ConstantTypeCode.String)
                    continue;

                var owner = $"{metadata.GetString(metadata.GetTypeDefinition(fieldDefinition.GetDeclaringType()).Name)}.{metadata.GetString(fieldDefinition.Name)}";
                var blob = metadata.GetBlobReader(constant.Value);
                if (blob.ReadConstant(constant.TypeCode) is string text && !string.IsNullOrEmpty(text))
                    yield return new(text, owner);
            }

            foreach (var methodDefinition in metadata.MethodDefinitions.Select(metadata.GetMethodDefinition))
            {
                if (methodDefinition.RelativeVirtualAddress == 0)
                    continue;

                var owner = $"{metadata.GetString(metadata.GetTypeDefinition(methodDefinition.GetDeclaringType()).Name)}.{metadata.GetString(methodDefinition.Name)}";
                var body = reader.GetMethodBody(methodDefinition.RelativeVirtualAddress);
                foreach (var text in ReadILBody(metadata, body))
                    yield return new(text, owner);
            }

            foreach (var customAttribute in metadata.CustomAttributes.Select(metadata.GetCustomAttribute))
            {
                switch (customAttribute.Constructor.Kind)
                {
                    case HandleKind.MethodDefinition:
                    {
                        var methodDefinition = metadata.GetMethodDefinition((MethodDefinitionHandle) customAttribute.Constructor);
                        var name = metadata.GetString(metadata.GetTypeDefinition((TypeDefinitionHandle) methodDefinition.GetDeclaringType()).Name);
                        foreach (var text in ReadCustomAttribute(metadata, reader.PEHeaders.PEHeader?.Magic == PEMagic.PE32 ? 4 : 8, customAttribute.Value, methodDefinition.Signature))
                            yield return new(text, name);
                        break;
                    }
                    case HandleKind.MemberReference:
                    {
                        var memberReference = metadata.GetMemberReference((MemberReferenceHandle) customAttribute.Constructor);
                        var name = metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle) memberReference.Parent).Name);
                        foreach (var text in ReadCustomAttribute(metadata, reader.PEHeaders.PEHeader?.Magic == PEMagic.PE32 ? 4 : 8, customAttribute.Value, memberReference.Signature))
                            yield return new(text, name);
                        break;
                    }
                }
            }
        }

        private static IEnumerable<string> ReadILBody(MetadataReader metadata, MethodBodyBlock body)
        {
            var ilReader = body.GetILReader();
            foreach (var codeInstruction in ILReader.GetInstructions(metadata, body))
            {
                if (codeInstruction.OperandType != OperandType.InlineString)
                    continue;

                ilReader.Offset = codeInstruction.OperandOffset;
                var metadataToken = ilReader.ReadInt32();
                string? text;
                try
                {
                    var userString = MetadataTokens.UserStringHandle(metadataToken);
                    text = metadata.GetUserString(userString);
                }
                catch (BadImageFormatException)
                {
                    text = null;
                }

                if (!string.IsNullOrEmpty(text))
                    yield return text;
            }
        }

        private static IEnumerable<string> ReadCustomAttribute(MetadataReader metadata, int ptrSize, BlobHandle valueBlob, BlobHandle signatureBlob)
        {
            var valueReader = metadata.GetBlobReader(valueBlob);
            var unknown = valueReader.ReadUInt16();
            var signatureReader = metadata.GetBlobReader(signatureBlob);
            var header = signatureReader.ReadSignatureHeader();
            var parameters = signatureReader.ReadCompressedInteger();
            var @return = signatureReader.ReadSignatureTypeCode();
            for (var i = 0; i < parameters; i++)
            {
                switch (signatureReader.ReadSignatureTypeCode())
                {
                    case SignatureTypeCode.Boolean:
                    case SignatureTypeCode.Byte:
                    case SignatureTypeCode.SByte:
                        valueReader.Offset += 1;
                        break;
                    case SignatureTypeCode.Char:
                    case SignatureTypeCode.Int16:
                    case SignatureTypeCode.UInt16:
                        valueReader.Offset += 2;
                        break;
                    case SignatureTypeCode.Int32:
                    case SignatureTypeCode.UInt32:
                    case SignatureTypeCode.Single:
                        valueReader.Offset += 4;
                        break;
                    case SignatureTypeCode.Int64:
                    case SignatureTypeCode.UInt64:
                    case SignatureTypeCode.Double:
                        valueReader.Offset += 8;
                        break;
                    case SignatureTypeCode.IntPtr:
                    case SignatureTypeCode.UIntPtr:
                        valueReader.Offset += ptrSize;
                        break;
                    case SignatureTypeCode.String:
                    {
                        var text = valueReader.ReadSerializedString();
                        if (!string.IsNullOrEmpty(text))
                            yield return text;
                        break;
                    }
                }
            }
        }
    }
}