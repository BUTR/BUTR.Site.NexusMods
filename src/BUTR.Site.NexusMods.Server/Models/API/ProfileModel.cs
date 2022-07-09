namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record ProfileModel(int UserId, string Name, string Email, string ProfileUrl, bool IsPremium, bool IsSupporter, string Role);
}