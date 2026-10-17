# BirSign webhook receiver integration

The experimental webhook HMAC/signing code was removed **before official release**. The durable BirSign sender sends unsigned POST JSON and retains `X-BirSign-Event-Id` and `EventId` in the payload.

- `AddBirSignAuthentication` (ASP.NET Core net8/9/10) and `UseBirSignAuthentication` (OWIN .NET Framework) preserve their original public method signatures and OIDC configuration.
- The original public constructors of `WebhookMiddleware` (3 parameters), `WebhookStartupFilter` (2 parameters), and `WebhookOwinMiddleware` (3 parameters) have been restored.
- No `BirSign:WebhookSigningSecret`, `BirSign.WebhookSigningSecret`, or `WebhookDelivery:SigningSecret` configuration is required; no HMAC signature classes, middleware parameters, or HMAC tests remain.
- The HMAC receiver/signature helper existed only on an experimental feature branch, not the baseline official API. Its removal therefore does **not** break existing officially released consumer integrations. Do not suggest users must upgrade existing apps to remove a feature they never received.
- OIDC `BirSign:ClientSecret`/client credentials remain REQUIRED where previously configured and are unrelated to webhook signing.
- Receivers do not authenticate webhook requests. Restrict ingress to trusted BirSign senders via firewall, reverse proxy, IP allowlist, or other network controls. Internal-only hosting is not by itself sender authentication.
- The sender uses at-least-once durable delivery with retry/backoff/dead-letter/replay; persist processed `EventId` transactionally with business changes for idempotency. Upserts in samples mitigate some duplicate attempts but do not provide transactional event deduplication.
- Verify: `dotnet run --project tests/WebhookProtocolTests/WebhookProtocolTests.csproj`; build `src/NetCoreExtension/NetCoreExtension.csproj` and `samples/MvcNetCore/MvcNetCore.csproj`.
- Legacy .NET Framework build may require manually restored packages and full MSBuild WebApplication targets.
- Wiki lives in a separate repository `https://github.com/FarshidRH/BirSignExtension.wiki.git` and should document the no-HMAC protocol without misleading migration instructions.