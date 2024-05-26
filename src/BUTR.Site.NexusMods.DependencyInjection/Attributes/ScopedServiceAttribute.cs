namespace BUTR.Site.NexusMods.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ScopedServiceAttribute<TInterface> : Attribute, IToRegister;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ScopedServiceAttribute<TInterface1, TInterface2> : Attribute, IToRegister;