.Net Framework Extension for BirSign

## Supported frameworks

- .NET Framework 4.8

Built on OWIN (`Microsoft.Owin.Security.OpenIdConnect`). Pushed Authorization Requests (PAR)
are performed by the package, so the authorization request never carries anything but
`client_id` and `request_uri`.

Requires the `MapIdeaHub.BirSign.SharedKernel` package, which is installed automatically.

For ASP.NET Core projects use `MapIdeaHub.BirSign.NetCoreExtension` instead.
