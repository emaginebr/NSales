---
name: dotnet-test-api
description: "Manages external API tests for .NET solutions using xUnit + Flurl.Http + FluentAssertions, with a shared IAsyncLifetime fixture that authenticates once per test session. Creates a dedicated <Solution>.ApiTests project (separate from <Solution>.Tests for unit tests). Default auth scheme is Generic JWT Bearer; NAuth, OAuth2 client-credentials, and API-key presets are documented. Use when the request mentions API tests, HTTP end-to-end tests, integration tests via HTTP, or external-endpoint validation. Do NOT use for unit tests — invoke `dotnet-test` instead."
allowed-tools: Read, Grep, Glob, Bash, Write, Edit, Task
user-invocable: true
---

# .NET API Test Project Manager (xUnit + Flurl + FluentAssertions)

You help developers create and maintain a dedicated `<Solution>.ApiTests` project for external API integration tests, cleanly separate from the unit-test project managed by the `dotnet-test` skill.

## Input

The user describes an intent like:

- `create API tests for ScratchApi` — boot a new `<Solution>.ApiTests` from scratch.
- `add tests for OrderController` — append `Controllers/OrderControllerTests.cs` and grow `Helpers/TestDataHelper.cs` on demand.
- `switch auth preset to NAuth` — rewrite the fixture using the NAuth preset.
- `run tests against staging` — guidance on overriding config via environment variables.

Before generating, classify the intent into one of these four flows.

## Pre-conditions

Before producing any file, read:

1. The solution file `<Solution>.sln` to enumerate projects.
2. Candidate **DTO projects** — see `## DTO Project Detection` for the suffix scan.
3. Whether `<Solution>.ApiTests/` already exists. If yes, **never overwrite** csproj, fixtures, or existing controller files — only append new ones.
4. Controller classes under `<Solution>.API/Controllers/` (or equivalent) to discover routes, `[Authorize]`/`[AllowAnonymous]` attributes, and DTOs used by public endpoints.
5. The target framework of the solution (default `net8.0`; if different, adjust package versions accordingly).

## Project Layout Convention

Single project per solution, named `<Solution>.ApiTests`, sitting at the solution root alongside `<Solution>.Tests`:

```text
<Solution>/
├── <Solution>.sln
├── <Solution>.Tests/                 ← unit tests (managed by `dotnet-test`)
└── <Solution>.ApiTests/              ← this skill
    ├── <Solution>.ApiTests.csproj
    ├── appsettings.Test.json         ← placeholders for secrets
    ├── Fixtures/
    │   ├── ApiTestFixture.cs
    │   └── ApiTestCollection.cs
    ├── Controllers/
    │   └── <Name>ControllerTests.cs  ← one per controller, added on demand
    └── Helpers/
        └── TestDataHelper.cs         ← grows per controller
```

One test class per controller. Test method name follows `<Method>_<Condition>_ShouldReturn<Expected>`.

## Dependencies

Canonical `<Solution>.ApiTests.csproj` packages (pin minimums; let the SDK resolve latest patch):

| Package | Minimum | Purpose |
|---|---|---|
| `xunit` | 2.5 | Test framework |
| `xunit.runner.visualstudio` | 2.5 | Test runner |
| `Microsoft.NET.Test.Sdk` | 17.8 | dotnet test entrypoint |
| `FluentAssertions` | 7.0 | Assertions (`.Should()`) |
| `Flurl.Http` | 4.0 | Fluent HTTP client |
| `Microsoft.Extensions.Configuration` | 9.0 | Config root |
| `Microsoft.Extensions.Configuration.Json` | 9.0 | `appsettings.Test.json` |
| `Microsoft.Extensions.Configuration.EnvironmentVariables` | 9.0 | env-var overrides |
| `coverlet.collector` | 6.0 | Coverage |

Project references: exactly **one** reference, to the detected DTO project (see next section). Never reference Domain, Application, Infra, or API.

## Secrets Policy

**No real credentials in `appsettings.Test.json`.** Every secret field MUST be the placeholder literal `REPLACE_VIA_ENV_<FullKey>`.

Env var override convention (`Microsoft.Extensions.Configuration.EnvironmentVariables`): nested keys use `__` (double underscore). Examples:

- `Auth:Email` → `Auth__Email`
- `Auth:Password` → `Auth__Password`
- `ApiBaseUrl` → `ApiBaseUrl` (no nesting)

At the end of the boot flow, emit a **"How to provide secrets"** block listing every `REPLACE_VIA_ENV_*` placeholder and how to export its corresponding env var — for bash, PowerShell, `.env` files, and GitHub Actions / Azure DevOps secrets.

Fast-fail is built into the fixture: if any resolved config value still starts with `REPLACE_VIA_ENV_`, `InitializeAsync` throws a descriptive exception naming the missing env var before any test runs.

## DTO Project Detection

Scan `<Solution>.sln` for sub-projects whose name ends with one of these suffixes, case-insensitive: `.DTO`, `.Dto`, `.Dtos`, `.Contracts`, `.Models`, `.Shared`.

Behaviour by candidate count:

- **0 candidates** — ask via `AskUserQuestion` whether to (a) supply a DTO project path manually, or (b) use inline payloads (records inside the test file). Proceed per choice.
- **1 candidate** — reference it automatically via `dotnet add <ApiTests>.csproj reference <Dto>.csproj`. Tell the user which project was chosen.
- **2+ candidates** — ask via `AskUserQuestion`, listing every candidate plus a final "none — use inline payloads" option.

## Default Auth Scheme — Generic JWT Bearer

Default `appsettings.Test.json`:

```json
{
  "ApiBaseUrl": "http://localhost:5000",
  "Auth": {
    "BaseUrl": "REPLACE_VIA_ENV_Auth__BaseUrl",
    "Email": "REPLACE_VIA_ENV_Auth__Email",
    "Password": "REPLACE_VIA_ENV_Auth__Password",
    "LoginEndpoint": "/auth/login"
  },
  "Timeout": 30
}
```

csproj must mark it `CopyToOutputDirectory: PreserveNewest`:

```xml
<ItemGroup>
  <Content Include="appsettings.Test.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

Default `Fixtures/ApiTestFixture.cs` (substitute `%%ROOT_NAMESPACE%%` with `<Solution>.ApiTests`):

```csharp
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;

namespace %%ROOT_NAMESPACE%%.Fixtures
{
    public class ApiTestFixture : IAsyncLifetime
    {
        public string BaseUrl { get; private set; } = string.Empty;
        public string AuthToken { get; private set; } = string.Empty;

        private IConfiguration _configuration = null!;

        public async Task InitializeAsync()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            BaseUrl = RequireConfig("ApiBaseUrl");
            var authBaseUrl = RequireConfig("Auth:BaseUrl");
            var email = RequireConfig("Auth:Email");
            var password = RequireConfig("Auth:Password");
            var loginEndpoint = _configuration["Auth:LoginEndpoint"] ?? "/auth/login";

            try
            {
                var response = await new Url(authBaseUrl)
                    .AppendPathSegment(loginEndpoint)
                    .PostJsonAsync(new { email, password })
                    .ReceiveJson<LoginResponse>();

                AuthToken = response?.Token ?? string.Empty;
                if (string.IsNullOrWhiteSpace(AuthToken))
                    throw new Exception($"Login at {authBaseUrl}{loginEndpoint} returned no token.");
            }
            catch (FlurlHttpException ex)
            {
                throw new Exception($"Failed to authenticate for API tests. Status: {ex.StatusCode}. " +
                    $"Ensure the auth API is running at {authBaseUrl} and credentials are correct.", ex);
            }
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public IFlurlRequest CreateAuthenticatedRequest(string path) =>
            new Url(BaseUrl).AppendPathSegment(path).WithOAuthBearerToken(AuthToken);

        public IFlurlRequest CreateAnonymousRequest(string path) =>
            new Url(BaseUrl).AppendPathSegment(path);

        private string RequireConfig(string key)
        {
            var value = _configuration[key] ?? throw new Exception($"Missing required config key '{key}'.");
            if (value.StartsWith("REPLACE_VIA_ENV_"))
            {
                var envVar = value.Substring("REPLACE_VIA_ENV_".Length);
                throw new Exception($"Config key '{key}' still holds the placeholder. " +
                    $"Export environment variable '{envVar}' before running the tests.");
            }
            return value;
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public bool Success { get; set; }
        }
    }
}
```

Default `Fixtures/ApiTestCollection.cs`:

```csharp
namespace %%ROOT_NAMESPACE%%.Fixtures
{
    [CollectionDefinition("ApiTests")]
    public class ApiTestCollection : ICollectionFixture<ApiTestFixture> { }
}
```

## Auth Presets

Pick one preset when the default (Generic JWT Bearer) doesn't match the target API's auth scheme. Each preset lists only the diff vs. the default.

### NAuth (tenant + device fingerprint JWT)

Adds to `appsettings.Test.json` under `Auth`: `Tenant`, `UserAgent`, `DeviceFingerprint` (the first is non-sensitive; the other two are identifier hints).

Adds to `ApiTestFixture.cs` (pseudo-diff):

```csharp
// Read extra config
_tenant = RequireConfig("Auth:Tenant");
_userAgent = _configuration["Auth:UserAgent"] ?? "ApiTests/1.0";
_deviceFingerprint = _configuration["Auth:DeviceFingerprint"] ?? "api-test-device";

// Add headers to login POST AND to both request helpers:
.WithHeader("X-Tenant-Id", _tenant)
.WithHeader("User-Agent", _userAgent)
.WithHeader("X-Device-Fingerprint", _deviceFingerprint)

// Use login path convention like `/user/loginWithEmail` instead of `/auth/login`.
```

### OAuth2 client credentials

Replaces `Email`/`Password` in `appsettings.Test.json` with `ClientId`/`ClientSecret`.

Changes `InitializeAsync`:

```csharp
var clientId = RequireConfig("Auth:ClientId");
var clientSecret = RequireConfig("Auth:ClientSecret");
var tokenEndpoint = _configuration["Auth:LoginEndpoint"] ?? "/oauth2/token";

var response = await new Url(authBaseUrl)
    .AppendPathSegment(tokenEndpoint)
    .PostUrlEncodedAsync(new {
        grant_type = "client_credentials",
        client_id = clientId,
        client_secret = clientSecret
    })
    .ReceiveJson<OAuthTokenResponse>();

AuthToken = response?.AccessToken ?? string.Empty;
```

### API key via header

Removes the whole login flow. `Auth` in `appsettings.Test.json` reduces to `{ "ApiKey": "REPLACE_VIA_ENV_Auth__ApiKey" }`.

`InitializeAsync` skips the POST. `CreateAuthenticatedRequest` changes to:

```csharp
private string _apiKey = string.Empty;
// In InitializeAsync: _apiKey = RequireConfig("Auth:ApiKey");

public IFlurlRequest CreateAuthenticatedRequest(string path) =>
    new Url(BaseUrl).AppendPathSegment(path).WithHeader("X-Api-Key", _apiKey);
```

## Creating the Project

Run the boot flow from the solution root:

```bash
# 1. Create the xUnit project
dotnet new xunit -n <Solution>.ApiTests -o <Solution>.ApiTests

# 2. Add to the solution
dotnet sln <Solution>.sln add <Solution>.ApiTests/<Solution>.ApiTests.csproj

# 3. Add packages (pin minimum versions from §Dependencies)
cd <Solution>.ApiTests
dotnet add package xunit --version 2.5.3
dotnet add package xunit.runner.visualstudio --version 2.5.3
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Flurl.Http --version 4.0.2
dotnet add package Microsoft.Extensions.Configuration --version 9.0.8
dotnet add package Microsoft.Extensions.Configuration.Json --version 9.0.8
dotnet add package Microsoft.Extensions.Configuration.EnvironmentVariables --version 9.0.8
dotnet add package coverlet.collector --version 6.0.0

# 4. Reference the detected DTO project (see §DTO Project Detection)
dotnet add reference ../<Solution>.DTO/<Solution>.DTO.csproj

# 5. Write the four files (appsettings.Test.json, Fixtures/ApiTestFixture.cs,
#    Fixtures/ApiTestCollection.cs, Helpers/TestDataHelper.cs empty shell)

# 6. Ensure the csproj has a <Using Include="Xunit" /> ItemGroup for Fact/Collection
#    and the Content block for appsettings.Test.json

# 7. Build once to surface any errors:
cd .. && dotnet build <Solution>.ApiTests/
```

After the build succeeds, emit the **"How to provide secrets"** instruction block (§Secrets Policy).

## Adding Tests for a Controller

For each `add tests for <Name>Controller` request:

1. Verify `<Solution>.ApiTests/` exists (if not, run the boot flow first).
2. Read `<Solution>.API/Controllers/<Name>Controller.cs` (or wherever controllers live). Enumerate public action methods, the HTTP verb, route template, parameters, return type, and authorization attribute.
3. Create `Controllers/<Name>ControllerTests.cs`:

```csharp
using FluentAssertions;
using Flurl.Http;
using %%ROOT_NAMESPACE%%.Fixtures;
using %%ROOT_NAMESPACE%%.Helpers;

namespace %%ROOT_NAMESPACE%%.Controllers
{
    [Collection("ApiTests")]
    public class <Name>ControllerTests
    {
        private readonly ApiTestFixture _fixture;
        public <Name>ControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        // For each [Authorize] endpoint, one anonymous-401 test AND one authenticated happy-path.
        // For each [AllowAnonymous] endpoint, one anonymous happy-path test.
        // Payloads come from TestDataHelper — never inline with ≥ 2 fields.
    }
}
```

4. Append the needed factories to `Helpers/TestDataHelper.cs` in alphabetical order — one `Create<DtoName>(...)` per request DTO the new tests use. **No orphan factories**: every method must be referenced by at least one `Controllers/*Tests.cs`.

### Assertion and request rules

- Use FluentAssertions (`.Should().Be(...)`). Never `Assert.Equal` / `Assert.True`.
- Use `.AllowAnyHttpStatus()` when asserting a specific status (otherwise Flurl throws on non-2xx).
- Use `AppendPathSegment` / `SetQueryParam` instead of string interpolation to compose URLs.

## Naming Conventions

- Test class: `<ControllerName>ControllerTests` in folder `Controllers/`.
- Test method: `<Method>_<Condition>_ShouldReturn<Expected>`. Examples:
  - `GetById_WithoutAuth_ShouldReturn401`
  - `GetById_WithAuth_ShouldNotReturn401`
  - `Search_WithAuth_ShouldReturnOk`
  - `Update_WithInvalidBody_ShouldReturn400`
- Factory: `TestDataHelper.Create<DtoName>(...)` — arguments are optional with sensible defaults for quick tests.

## Running the Tests

Local run:

```bash
# Export required env vars first (see §Secrets Policy).
# Bash:
export ApiBaseUrl=http://localhost:5000
export Auth__BaseUrl=https://localhost:5001/auth-api
export Auth__Email=qa@example.com
export Auth__Password=<secret>

# PowerShell:
$env:ApiBaseUrl = 'http://localhost:5000'
$env:Auth__BaseUrl = 'https://localhost:5001/auth-api'
$env:Auth__Email = 'qa@example.com'
$env:Auth__Password = '<secret>'

dotnet test <Solution>.ApiTests/
```

GitHub Actions snippet (add secrets under `Settings → Secrets and variables → Actions`):

```yaml
- name: Run API tests
  env:
    ApiBaseUrl: ${{ vars.API_BASE_URL }}
    Auth__BaseUrl: ${{ vars.AUTH_BASE_URL }}
    Auth__Email: ${{ secrets.AUTH_EMAIL }}
    Auth__Password: ${{ secrets.AUTH_PASSWORD }}
  run: dotnet test <Solution>.ApiTests/ --logger "trx;LogFileName=api-tests.trx"
```

If any `REPLACE_VIA_ENV_*` placeholder remains in the resolved config at runtime, the fixture throws at start-up naming the missing variable — no partial run.

## Boundaries

- **Production code**: never modified. If a controller requires changes to be testable, defer to `dotnet-senior-developer` and pause.
- **Unit tests** of services, factories, domain entities, utilities: out of scope — invoke `dotnet-test` instead.
- **Non-.NET backends** (Python, Node, Go, Java): out of scope.
- **.NET Framework legacy** (non-SDK-style csproj, `packages.config`): out of scope — only SDK-style .NET 6/7/8/9 supported.
- **Test orchestration** (running the API under test, spinning Docker Compose, provisioning DB): out of scope — the skill assumes the API is reachable at `ApiBaseUrl`.
