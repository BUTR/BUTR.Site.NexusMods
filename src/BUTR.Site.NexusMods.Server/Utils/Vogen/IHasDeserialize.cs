namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public interface IHasDeserialize<out TVogen, in TValueObject>
{
    static abstract TVogen DeserializeDangerous(TValueObject instance);
}