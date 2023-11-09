using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System;
using System.ComponentModel.DataAnnotations;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class BindTenantAttribute : ValidationAttribute, IBindingSourceMetadata, IModelNameProvider, IFromHeaderMetadata, IBindIgnore
{
    public BindingSource BindingSource => BindingSource.Header;
    public string Name => "Tenant";

    public override bool IsValid(object? value) => value is not null;
}