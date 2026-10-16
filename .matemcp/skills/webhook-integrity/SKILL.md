# BirSign webhook receiver integration

The BirSign durable sender now sends **unsigned** POST JSON. It retains `X-BirSign-Event-Id`, and each event JSON includes `EventId`. No shared Webhook SigningSecret is required.

- `AddBirSignAuthentication` (ASP.NET Core, net8/9/10) and `UseBirSignAuthentication` (.NET Framework OWIN) keep their public signatures.
- Both receiver middlewares accept unsigned events, including when legacy `BirSign:WebhookSigningSecret` / `BirSign.WebhookSigningSecret` settings remain configured.
- Do not pass webhook signing settings from authentication setup into middleware. The old optional middleware constructor parameters and public SharedKernel `WebhookSignature` helper remain solely to avoid source/binary API breaks; they do not enforce or signal request authenticity.
- **Behavioral breaking change across deployments**: old consumers configured with HMAC verification reject the new sender's unsigned events with HTTP 401. Upgrade them before switching the sender, or remove obsolete webhook-specific secret settings on old consumers as a temporary migration step.
- Continue to configure the unrelated OIDC `BirSign:ClientSecret` and existing client credentials.
- The receivers provide no webhook authentication. Restrict ingress to the trusted sender at reverse proxy/firewall/network level. Never assume `[Authorize]` protects middleware that executes before auth.
- The sender is at-least-once with durable retries; persist processed `EventId` transactionally with business updates for exactly-once business effects. Simple entity upserts in samples only mitigate a subset of duplicate attempts.
- Tests: `dotnet run --project tests/WebhookProtocolTests/WebhookProtocolTests.csproj`; build `src/NetCoreExtension/NetCoreExtension.csproj` and `samples/MvcNetCore/MvcNetCore.csproj` for net8/9/10 compatibility.
- Full .NET Framework build validation may require Windows .NET Framework/MSBuild WebApplication targets and restored NuGet packages.
- Wiki repository is separate (`https://github.com/FarshidRH/BirSignExtension.wiki.git`); Webhooks, Troubleshooting, and Changelog/Migration must stay aligned with this protocol.