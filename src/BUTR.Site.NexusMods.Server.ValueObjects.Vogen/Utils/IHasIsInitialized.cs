namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IHasIsInitialized<in TVogen>
{
    static abstract bool IsInitialized(TVogen instance);
}