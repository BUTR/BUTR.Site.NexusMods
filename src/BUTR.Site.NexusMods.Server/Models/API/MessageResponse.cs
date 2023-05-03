namespace BUTR.Site.NexusMods.Server.Models.API;

public abstract record APIResponse
{
    public static APIResponse<T?> From<T>(T? data, string error = "") => new(data, error);
    public static APIResponse<T?> Error<T>(string error) => new(default, error);
}
public record APIResponse<T>(T? Data, string HumanReadableError) : APIResponse;