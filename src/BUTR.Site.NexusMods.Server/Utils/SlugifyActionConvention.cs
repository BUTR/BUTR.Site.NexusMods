using Microsoft.AspNetCore.Mvc.ApplicationModels;

using System.Text.RegularExpressions;

namespace BUTR.Site.NexusMods.Server.Utils;

public partial class SlugifyActionConvention : IActionModelConvention
{
    [GeneratedRegex("([a-z])([A-Z])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex SlugifyRegex();

    public void Apply(ActionModel action)
    {
        //action.ActionName = SlugifyRegex().Replace(action.ActionName, "$1-$2").ToLowerInvariant();
        foreach (var actionSelector in action.Selectors)
        {
            if (actionSelector.AttributeRouteModel?.Template is not null)
                actionSelector.AttributeRouteModel.Template = SlugifyRegex().Replace(actionSelector.AttributeRouteModel.Template, "$1-$2").ToLowerInvariant();
        }
        //action.Controller.ControllerName = SlugifyRegex().Replace(action.Controller.ControllerName, "$1-$2").ToLowerInvariant();
    }
}