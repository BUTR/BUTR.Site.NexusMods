using ICSharpCode.Decompiler.Metadata;

using System.Reflection.Metadata;

namespace BUTR.Site.NexusMods.Server.Utils.Reflection;

// Test implementation of ISignatureTypeProvider<TType, TGenericContext> that uses strings in ilasm syntax as TType.
// A real provider in any sort of perf constraints would not want to allocate strings freely like this, but it keeps test code simple.
internal class DisassemblingTypeProvider : StringTypeProviderBase<MetadataGenericContext>
{
    public static readonly MetadataGenericContext EmptyContext = new();
        
    public override SignatureType GetGenericMethodParameter(MetadataGenericContext genericContext, int index)
    {
        //var handle = genericContext.GetGenericMethodTypeParameterHandleOrNull(index);
        //var parameter = _metadataReader.GetGenericParameter(handle);
        //var name = _metadataReader.GetString(parameter.Name);
        //var name2 = genericContext.GetGenericMethodTypeParameterName(index);
        return SignatureType.FromString(genericContext.GetGenericMethodTypeParameterName(index));
    }

    public override SignatureType GetGenericTypeParameter(MetadataGenericContext genericContext, int index)
    {
        //var handle = genericContext.GetGenericTypeParameterHandleOrNull(index);
        //var parameter = _metadataReader.GetGenericParameter(handle);
        //var name = _metadataReader.GetString(parameter.Name);
        //var name2 = genericContext.GetGenericTypeParameterName(index);
        return SignatureType.FromString(genericContext.GetGenericTypeParameterName(index));
    }

    public override SignatureType GetTypeFromSpecification(MetadataReader reader, MetadataGenericContext _, TypeSpecificationHandle handle, byte rawTypeKind)
    {
        return MetadataNameFormatter.FormatHandle(reader, handle);
    }
}