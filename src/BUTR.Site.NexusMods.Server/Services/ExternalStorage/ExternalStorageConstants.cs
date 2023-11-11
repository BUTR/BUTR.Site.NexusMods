namespace BUTR.Site.NexusMods.Server.Services;

public record ExternalDataHolder<TData>(string ExternalId, TData Data) where TData : class;