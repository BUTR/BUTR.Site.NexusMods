namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public interface IHasDefaultValue<out TVogen>
{
    public static abstract TVogen DefaultValue { get; }
}