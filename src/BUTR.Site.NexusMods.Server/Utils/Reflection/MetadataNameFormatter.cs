using ICSharpCode.Decompiler.Metadata;

using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace BUTR.Site.NexusMods.Server.Utils.Reflection;

/// <summary>
/// Helper class for converting metadata tokens into their textual representation.
/// Taken from https://github.com/dotnet/runtime/blob/f179b7634370fc9181610624cc095370ec53e072/src/coreclr/tools/aot/ILCompiler.Reflection.ReadyToRun/ReadyToRunSignature.cs#L70
/// </summary>
internal class MetadataNameFormatter : DisassemblingTypeProvider
{
    private readonly MetadataReader _metadataReader;

    private MetadataNameFormatter(MetadataReader metadataReader)
    {
        _metadataReader = metadataReader;
    }

    public static SignatureType FormatHandle(MetadataReader metadataReader, Handle handle, bool namespaceQualified = true, SignatureType? owningTypeOverride = null, MetadataGenericContext? genericContext = null, string signaturePrefix = "")
    {
        var formatter = new MetadataNameFormatter(metadataReader);
        return formatter.EmitHandleName(handle, namespaceQualified, owningTypeOverride, genericContext, signaturePrefix);
    }

    private SignatureType EmitHandleName(Handle handle, bool namespaceQualified, SignatureType? owningTypeOverride, MetadataGenericContext? genericContext, string signaturePrefix = "")
    {
        try
        {
            return handle.Kind switch
            {
                HandleKind.MemberReference => EmitMemberReferenceName((MemberReferenceHandle) handle, owningTypeOverride, genericContext, signaturePrefix),
                HandleKind.MethodSpecification => EmitMethodSpecificationName((MethodSpecificationHandle) handle, owningTypeOverride, genericContext, signaturePrefix),
                HandleKind.MethodDefinition => EmitMethodDefinitionName((MethodDefinitionHandle) handle, owningTypeOverride, genericContext, signaturePrefix),
                HandleKind.TypeReference => EmitTypeReferenceName((TypeReferenceHandle) handle, namespaceQualified, genericContext, signaturePrefix),
                HandleKind.TypeSpecification => EmitTypeSpecificationName((TypeSpecificationHandle) handle, namespaceQualified, genericContext, signaturePrefix),
                HandleKind.TypeDefinition => EmitTypeDefinitionName((TypeDefinitionHandle) handle, namespaceQualified, genericContext, signaturePrefix),
                HandleKind.FieldDefinition => EmitFieldDefinitionName((FieldDefinitionHandle) handle, namespaceQualified, owningTypeOverride, genericContext, signaturePrefix),
                _ => throw new NotImplementedException(),
            };
        }
        catch (Exception ex)
        {
            return SignatureType.FromString($"$$INVALID-{handle.Kind}-{MetadataTokens.GetRowNumber((EntityHandle) handle):X6}: {ex.Message}");
        }
    }

    private void ValidateHandle(EntityHandle handle, TableIndex tableIndex)
    {
        var rowid = MetadataTokens.GetRowNumber(handle);
        var tableRowCount = _metadataReader.GetTableRowCount(tableIndex);
        if (rowid <= 0 || rowid > tableRowCount)
        {
            throw new NotImplementedException($"Invalid handle {MetadataTokens.GetToken(handle):X8} in table {tableIndex} ({tableRowCount} rows)");
        }
    }

    private SignatureType EmitMethodSpecificationName(MethodSpecificationHandle methodSpecHandle, SignatureType? owningTypeOverride, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(methodSpecHandle, TableIndex.MethodSpec);
        var methodSpec = _metadataReader.GetMethodSpecification(methodSpecHandle);
        return EmitHandleName(methodSpec.Method, namespaceQualified: true, owningTypeOverride: owningTypeOverride, genericContext, signaturePrefix: signaturePrefix);
    }

    private SignatureType EmitMemberReferenceName(MemberReferenceHandle memberRefHandle, SignatureType? owningTypeOverride, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(memberRefHandle, TableIndex.MemberRef);
        var memberRef = _metadataReader.GetMemberReference(memberRefHandle);
        var sb = new StringBuilder();
        switch (memberRef.GetKind())
        {
            case MemberReferenceKind.Field:
            {
                var fieldSig = memberRef.DecodeFieldSignature(this, genericContext ?? EmptyContext);
                sb.Append(fieldSig);
                sb.Append(' ');
                sb.Append(EmitContainingTypeAndMemberName(memberRef, owningTypeOverride, genericContext, signaturePrefix));
                break;
            }

            case MemberReferenceKind.Method:
            {
                var methodSig = memberRef.DecodeMethodSignature(this, genericContext ?? EmptyContext);
                sb.Append(methodSig.ReturnType);
                sb.Append(' ');
                sb.Append(EmitContainingTypeAndMemberName(memberRef, owningTypeOverride, genericContext, signaturePrefix));
                sb.Append(EmitMethodSignature(methodSig));
                break;
            }

            default:
                throw new NotImplementedException(memberRef.GetKind().ToString());
        }

        return SignatureType.FromString(sb.ToString());
    }

    private SignatureType EmitMethodDefinitionName(MethodDefinitionHandle methodDefinitionHandle, SignatureType? owningTypeOverride, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(methodDefinitionHandle, TableIndex.MethodDef);
        var methodDef = _metadataReader.GetMethodDefinition(methodDefinitionHandle);
        var methodSig = methodDef.DecodeSignature(this, genericContext ?? new MetadataGenericContext(methodDefinitionHandle, _metadataReader));
        var sb = new StringBuilder()
            .Append(methodSig.ReturnType)
            .Append(' ')
            .Append(owningTypeOverride ?? EmitHandleName(methodDef.GetDeclaringType(), namespaceQualified: true, owningTypeOverride: null, genericContext))
            .Append('.')
            .Append(signaturePrefix)
            .Append(EmitString(methodDef.Name))
            .Append(EmitMethodSignature(methodSig));
        return SignatureType.FromString(sb.ToString());
    }

    private SignatureType EmitMethodSignature(MethodSignature<SignatureType> methodSignature)
    {
        var sb = new StringBuilder();
        if (methodSignature.GenericParameterCount != 0)
        {
            sb.Append('<');
            var firstTypeArg = true;
            for (var typeArgIndex = 0; typeArgIndex < methodSignature.GenericParameterCount; typeArgIndex++)
            {
                if (firstTypeArg)
                {
                    firstTypeArg = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append("!!");
                sb.Append(typeArgIndex);
            }
            sb.Append('>');
        }
        sb.Append('(');
        var firstMethodArg = true;
        foreach (var paramType in methodSignature.ParameterTypes)
        {
            if (firstMethodArg)
            {
                firstMethodArg = false;
            }
            else
            {
                sb.Append(", ");
            }
            sb.Append(paramType);
        }
        sb.Append(')');
        return SignatureType.FromString(sb.ToString());
    }

    private SignatureType EmitContainingTypeAndMemberName(MemberReference memberRef, SignatureType? owningTypeOverride, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        owningTypeOverride ??= EmitHandleName(memberRef.Parent, namespaceQualified: true, owningTypeOverride: null, genericContext);
        return owningTypeOverride with { Name = $"{owningTypeOverride.Name}.{signaturePrefix}{EmitString(memberRef.Name)}" }; // No idea what emits this
    }

    private SignatureType EmitTypeReferenceName(TypeReferenceHandle typeRefHandle, bool namespaceQualified, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(typeRefHandle, TableIndex.TypeRef);
        var typeRef = _metadataReader.GetTypeReference(typeRefHandle);
        if (typeRef.ResolutionScope.Kind != HandleKind.AssemblyReference)
        {
            // Nested type - format enclosing type followed by the nested type
            var originalType = EmitHandleName(typeRef.ResolutionScope, namespaceQualified, owningTypeOverride: null, genericContext);
            return originalType with { IsNested = true, Name = $"{originalType.Name}+{EmitString(typeRef.Name)}" };
        }
        var sb = new StringBuilder();
        if (namespaceQualified)
        {
            sb.Append(EmitString(typeRef.Namespace));
            if (sb.Length > 0) sb.Append('.');
        }
        sb.Append(signaturePrefix).Append(EmitString(typeRef.Name));
        return SignatureType.FromString(sb.ToString());
    }

    private SignatureType EmitTypeDefinitionName(TypeDefinitionHandle typeDefHandle, bool namespaceQualified, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(typeDefHandle, TableIndex.TypeDef);
        var typeDef = _metadataReader.GetTypeDefinition(typeDefHandle);
        if (typeDef.IsNested)
        {
            // Nested type
            var originalType = EmitHandleName(typeDef.GetDeclaringType(), namespaceQualified, owningTypeOverride: null, genericContext);
            return originalType with { IsNested = true, Name = $"{originalType.Name}+{signaturePrefix}{EmitString(typeDef.Name)}" };
        }

        var sb = new StringBuilder();
        if (namespaceQualified)
        {
            sb.Append(EmitString(typeDef.Namespace));
            if (sb.Length > 0) sb.Append('.');
        }
        sb.Append(signaturePrefix).Append(EmitString(typeDef.Name));
        return SignatureType.FromString(sb.ToString());
    }

    private SignatureType EmitTypeSpecificationName(TypeSpecificationHandle typeSpecHandle, bool namespaceQualified, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(typeSpecHandle, TableIndex.TypeSpec);
        var typeSpec = _metadataReader.GetTypeSpecification(typeSpecHandle);
        return typeSpec.DecodeSignature(this, genericContext ?? EmptyContext);
    }

    private SignatureType EmitFieldDefinitionName(FieldDefinitionHandle fieldDefHandle, bool namespaceQualified, SignatureType? owningTypeOverride, MetadataGenericContext? genericContext, string signaturePrefix)
    {
        ValidateHandle(fieldDefHandle, TableIndex.Field);
        var fieldDef = _metadataReader.GetFieldDefinition(fieldDefHandle);
        var sb = new StringBuilder()
            .Append(fieldDef.DecodeSignature(this, genericContext ?? EmptyContext))
            .Append(' ')
            .Append(EmitHandleName(fieldDef.GetDeclaringType(), namespaceQualified, owningTypeOverride, genericContext))
            .Append('.')
            .Append(signaturePrefix)
            .Append(_metadataReader.GetString(fieldDef.Name));
        return SignatureType.FromString(sb.ToString());
    }

    private string EmitString(StringHandle handle)
    {
        return  _metadataReader.GetString(handle);
    }
}