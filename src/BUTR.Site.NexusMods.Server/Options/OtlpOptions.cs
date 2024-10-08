using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using OpenTelemetry.Exporter;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class OtlpOptionsValidator : AbstractValidator<OtlpOptions>
{
    public OtlpOptionsValidator()
    {
        RuleFor(static x => x.LoggingEndpoint).IsUri().IsUrlTcpEndpointAvailable().When(static x => !string.IsNullOrEmpty(x.LoggingEndpoint));
        RuleFor(static x => x.TracingEndpoint).IsUri().IsUrlTcpEndpointAvailable().When(static x => !string.IsNullOrEmpty(x.TracingEndpoint));
        RuleFor(static x => x.MetricsEndpoint).IsUri().IsUrlTcpEndpointAvailable().When(static x => !string.IsNullOrEmpty(x.MetricsEndpoint));
    }
}

public sealed record OtlpOptions
{
    public required string LoggingEndpoint { get; init; } = default!;
    public required OtlpExportProtocol LoggingProtocol { get; init; } = default!;
    public required string TracingEndpoint { get; init; } = default!;
    public required OtlpExportProtocol TracingProtocol { get; init; } = default!;
    public required string MetricsEndpoint { get; init; } = default!;
    public required OtlpExportProtocol MetricsProtocol { get; init; } = default!;
}