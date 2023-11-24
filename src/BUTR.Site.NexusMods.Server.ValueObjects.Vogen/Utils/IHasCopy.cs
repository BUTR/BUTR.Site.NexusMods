namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IHasCopy<TVogen>
{
    static abstract TVogen Copy(TVogen instance);
}