namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IHasDefaultValue<out TVogen>
{
    public static abstract TVogen DefaultValue { get; }
}