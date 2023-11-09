using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

public class ClaimsBindingSource : BindingSource
{
    public static ClaimsBindingSource BindingSource => new();
    private ClaimsBindingSource() : base("Claims", "Claims", false, true) { }
}