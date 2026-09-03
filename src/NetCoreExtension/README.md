.Net Core Extension for BirSign

## Supported frameworks

- .NET 8 (LTS)
- .NET 9
- .NET 10 (LTS)

Pushed Authorization Requests (PAR) are used on every target. On .NET 9 and later this is the
built-in ASP.NET Core support; on .NET 8 the package supplies an equivalent implementation, so
the authorization request never carries anything but `client_id` and `request_uri`.

Requires the `MapIdeaHub.BirSign.SharedKernel` package, which is installed automatically.

## Applications that already own a sign-in cookie

By default `AddBirSignAuthentication` owns the session: it registers its own cookie scheme,
points the application's default schemes at it, and handles back-channel logout there. That
suits an application with no authentication of its own.

An application built on ASP.NET Core Identity already has a cookie scheme, and repointing the
defaults would send every `[Authorize]` to the wrong one. Pass `signInScheme` instead:

```csharp
builder.Services.AddBirSignAuthentication(
    builder.Configuration,
    signInScheme: IdentityConstants.ExternalScheme);
```

The package then adds nothing but the OpenID Connect handler. Completing the sign-in — reading
the external cookie, resolving the local user, calling `SignInManager.SignInAsync` — and
invalidating a session on back-channel logout become the application's job, which is what it
wants anyway: its own user table stays the anchor for its foreign keys and its own roles.
