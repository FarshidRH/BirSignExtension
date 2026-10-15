# BirSign webhook receiver integration

The existing authentication extension installs the webhook receiver automatically when a webhook handler is supplied.

- ASP.NET Core: AddBirSignAuthentication keeps the same method signature; its receiver reads BirSign:WebhookSigningSecret from IConfiguration.
- .NET Framework OWIN: UseBirSignAuthentication keeps the same method signature; its receiver reads BirSign.WebhookSigningSecret from appSettings.
- With a non-empty secret, the middleware verifies the X-BirSign-Timestamp and X-BirSign-Signature headers (HMAC SHA-256 of timestamp + dot + raw JSON body) and rejects missing/invalid/stale signatures.
- Without a configured secret, existing consumers remain backward-compatible, but incoming webhook requests are NOT authenticated.
- Receiver handlers should deduplicate by EventId; the server has at-least-once delivery semantics.
- Test: dotnet run --project tests/WebhookProtocolTests/WebhookProtocolTests.csproj
- NetCoreExtension supports net8.0, net9.0 and net10.0.
- The legacy NetFrameworkExtension is retained, but full build validation may require Windows .NET Framework/MSBuild WebApplication targets and the existing NuGet packages.
- Do not reuse a shared signing secret across independently trusted tenants. Per-client key management is a future hardening requirement.