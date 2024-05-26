global using BUTR.Site.NexusMods.Server.ValueObjects.Utils;
global using BUTR.Site.NexusMods.Shared.Helpers;

global using System.Diagnostics.CodeAnalysis;
global using System.Diagnostics.Contracts;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;

global using Vogen;

/*
[assembly: VogenDefaults(
    isInitializedMethodGeneration: IsInitializedMethodGeneration.Generate,
    systemTextJsonConverterFactoryGeneration: SystemTextJsonConverterFactoryGeneration.Generate,
    staticAbstractsGeneration: StaticAbstractsGeneration.ExplicitCastFromPrimitive |
                               StaticAbstractsGeneration.ExplicitCastToPrimitive |
                               //StaticAbstractsGeneration.ImplicitCastToPrimitive |
                               StaticAbstractsGeneration.EqualsOperators |
                               StaticAbstractsGeneration.FactoryMethods |
                               StaticAbstractsGeneration.InstanceMethodsAndProperties,
    openApiSchemaCustomizations: OpenApiSchemaCustomizations.GenerateSwashbuckleSchemaFilter)]
*/