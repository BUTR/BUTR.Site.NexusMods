namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public interface IHasCopy<TVogen>
{
    static abstract TVogen Copy(TVogen instance);
}