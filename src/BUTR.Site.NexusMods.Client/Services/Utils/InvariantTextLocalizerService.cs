using Blazorise.Localization;

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class InvariantTextLocalizerService : ITextLocalizerService
{
    public CultureInfo SelectedCulture => CultureInfo.InvariantCulture;

    public IEnumerable<CultureInfo> AvailableCultures => new[] { CultureInfo.InvariantCulture };

    public event EventHandler? LocalizationChanged;

    public void AddLanguageResource(string cultureName) { }
    public void ChangeLanguage(string cultureName, bool changeThreadCulture = true) { }
}