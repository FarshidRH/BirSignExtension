# MVC Net Core integration sample

This sample uses two different local BirSign services. **Do not point the management API at the Admin UI.**

| Service | Development URL | Purpose |
| --- | --- | --- |
| IdentityServer STS | `https://localhost:44310` | OIDC login, client-credentials access tokens |
| Admin API | `https://localhost:44302` | `ManageChartApi`, roles and user-management endpoints |
| Admin UI | `https://localhost:44303` | Browser administration interface (not an API endpoint) |
| MVC sample | `https://localhost:7091` | Client and local chart database |

## Local configuration

Create or update `samples/MvcNetCore/appsettings.Development.json` **locally**. This file is gitignored: it contains developer-specific settings and credentials. For the chart API, use:

```json
{
  "BirSign": {
    "Authority": "https://localhost:44310",
    "ApiUri": "https://localhost:44302",
    "ClientId": "YOUR_TEST_CLIENT_ID",
    "ClientSecret": "YOUR_LOCAL_TEST_CLIENT_SECRET"
  }
}
```

Use the actual registered test-client values. Other OIDC settings (callback paths etc.) can inherit from `appsettings.json` if unchanged. The registered OIDC redirect URI must match `https://localhost:7091/BirSignIn`. `ClientSecret` is used for **OIDC and API access**, not for HMAC webhook signatures.

`/Chart/Sync` calls `IdsService.GetDepartmentTreeWithDetailsAsync()`, which requests the `get_chart_data_scope` client-credentials token and then makes an authenticated GET request to:

```
https://localhost:44302/Api/ManageChartApi/GetDepartmentTreeWithDetails
```

In the BirSign administration database, the **Software** entry associated with the test client must have `ChartParentDepartmentId` set to the department whose descendants should be synchronized. Without it, the API returns `ChartParentDepartmentNotSet` (an error JSON response, potentially with HTTP 200); the MVC application cannot import the tree. Choose the narrowest appropriate root, not the entire municipal hierarchy, for testing.

For an OIDC login to succeed, the signed-in account must hold a role for that Software entry (by direct assignment or organizational position) or be covered by an appropriate public role, e.g. the non-admin `User` role marked `IsPublicForAll` **only when every authenticated user is supposed to access this test application**. Do not make `Admin` public.

## Troubleshooting

- `404 Not Found` from chart API: check that `BirSign:ApiUri` is `https://localhost:44302`, **not** `https://localhost:44303`.
- `401 Unauthorized` when calling chart API without a bearer token is expected; verify the service account has `get_chart_data_scope`.
- `ChartParentDepartmentNotSet`: configure the test Software entry's organizational root in BirSign.
- Browser `ERR_TOO_MANY_REDIRECTS` between `/Account/Login` and `/connect/authorize/callback`: verify the test user's Software role/eligibility and the IdentityServer profile-service logs, not just the OIDC client ID/secret.
- Webhook delivery is separate from manual chart sync. New webhook POSTs are unsigned and require network-level sender restrictions and consumer `EventId` deduplication.

No changes to already deployed BirSign-connected systems are necessary for these **sample-only development settings**.