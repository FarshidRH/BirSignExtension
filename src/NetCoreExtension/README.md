.Net Core Extension for BirSign

## Supported frameworks

- .NET 8 (LTS)
- .NET 9
- .NET 10 (LTS)

Pushed Authorization Requests (PAR) are used on every target. On .NET 9 and later this is the
built-in ASP.NET Core support; on .NET 8 the package supplies an equivalent implementation, so
the authorization request never carries anything but `client_id` and `request_uri`.

Requires the `MapIdeaHub.BirSign.SharedKernel` package, which is installed automatically.
