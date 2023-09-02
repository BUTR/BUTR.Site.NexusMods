namespace BUTR.Site.NexusMods.Server.Services;

public class ExternalStorageConstants
{
    public static readonly string Discord = "DiscordTokens";
    public static readonly string Steam = "SteamTokens";
    public static readonly string GOG = "GOGTokens";
}

public record ExternalDataHolder<TData>(string ExternalId, TData Data) where TData : class;