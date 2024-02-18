namespace BUTR.Site.NexusMods.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TransientServiceAttribute<TInterface> : Attribute, IToRegister { }