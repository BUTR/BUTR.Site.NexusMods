namespace BUTR.Site.NexusMods.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class HostedServiceAttribute : Attribute, IToRegister { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class HostedServiceAttribute<TInterface> : Attribute, IToRegister { }