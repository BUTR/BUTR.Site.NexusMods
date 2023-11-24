namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IHasDeserialize<out TVogen, in TValueObject>
{
    static abstract TVogen DeserializeDangerous(TValueObject instance);
}