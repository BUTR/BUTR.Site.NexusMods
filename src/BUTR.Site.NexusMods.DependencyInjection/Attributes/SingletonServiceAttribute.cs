namespace BUTR.Site.NexusMods.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SingletonServiceAttribute : Attribute, IToRegister;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SingletonServiceAttribute<TInterface> : Attribute, IToRegister;