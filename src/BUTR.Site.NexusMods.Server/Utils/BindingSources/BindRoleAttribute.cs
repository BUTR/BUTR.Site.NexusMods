using BUTR.Authentication.NexusMods.Authentication;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using System;
using System.ComponentModel.DataAnnotations;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class BindRoleAttribute : ValidationAttribute, IBindingSourceMetadata, IModelNameProvider, IBindIgnore
{
    public BindingSource BindingSource => ClaimsBindingSource.BindingSource;
    public string Name => ButrNexusModsClaimTypes.Role;

    public override bool IsValid(object? value) => value is not null;
}