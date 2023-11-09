namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public interface IHasIsInitialized<in TVogen>
{
    static abstract bool IsInitialized(TVogen instance);
}