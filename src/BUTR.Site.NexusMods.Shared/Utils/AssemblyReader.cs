using Mono.Cecil;
using Mono.Cecil.Rocks;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BUTR.Site.NexusMods.Shared.Utils
{
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
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyStream);

            foreach (var fieldDefinition in assemblyDefinition.Modules.SelectMany(x => x.Types.SelectMany(y => y.Fields)))
            {
                if (!fieldDefinition.Attributes.HasFlag(FieldAttributes.Literal))
                    continue;

                if (fieldDefinition.Constant is not string text)
                    continue;

                var owner = $"{fieldDefinition.DeclaringType.Name}.{fieldDefinition.Name}";
                if (!string.IsNullOrEmpty(text))
                    yield return new(text, owner);
            }

            foreach (var methodDefinition in assemblyDefinition.Modules.SelectMany(x => x.Types.SelectMany(y => y.Methods)))
            {
                if (!methodDefinition.HasBody)
                    continue;

                var owner = $"{methodDefinition.Name}.{methodDefinition.Name}";
                foreach (var codeInstruction in methodDefinition.Body.Instructions)
                {
                    if (codeInstruction.Operand is not string text)
                        continue;

                    if (!string.IsNullOrEmpty(text))
                        yield return new(text, owner);
                }
            }

            var attributes = assemblyDefinition.CustomAttributes
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.CustomAttributes))
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.GetAllTypes().SelectMany(y => y.CustomAttributes)))
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.GetAllTypes().SelectMany(y => y.Fields).SelectMany(y => y.CustomAttributes)))
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.GetAllTypes().SelectMany(y => y.Properties).SelectMany(y => y.CustomAttributes)))
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.GetAllTypes().SelectMany(y => y.Methods).SelectMany(y => y.CustomAttributes)))
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.GetAllTypes().SelectMany(y => y.Events).SelectMany(y => y.CustomAttributes)))
                .Concat(assemblyDefinition.Modules.SelectMany(x => x.GetAllTypes().SelectMany(y => y.Interfaces).SelectMany(y => y.CustomAttributes)));
            foreach (var customAttribute in attributes)
            {
                var owner = $"{customAttribute.AttributeType.Name}";
                var args = customAttribute.ConstructorArguments.Concat(customAttribute.Properties.Select(x => x.Argument)).Concat(customAttribute.Fields.Select(x => x.Argument));
                foreach (var constructorArgument in args)
                {
                    if (constructorArgument.Value is not string text)
                        continue;

                    if (!string.IsNullOrEmpty(text))
                        yield return new(text, owner);
                }
            }
        }
    }
}