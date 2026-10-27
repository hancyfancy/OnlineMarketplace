# Online Marketplace

A .NET 8 marketplace solution backed by SQL Server. It contains a web application for buyers and sellers and a REST API, both sharing one database (`MarketplaceDB`).

| Project | Purpose | Authentication |
|---|---|---|
| `source/Web` | ASP.NET Core MVC (areas `Identity`, `Private`, `Public`) and Razor Pages web application | Google OAuth, cookie session |
| `source/Api` | Versioned REST API with Swagger (development only) | JWT bearer token |

Supporting projects: `source/Application` (use cases, MediatR), `source/Domain` (entities and abstractions), `source/Infrastructure` (EF Core, SQL Server, Identity, seeding), `source/Composition` (service registration and JWT token creation). Tests are in `test/Unit`.

## Contents

1. [Prerequisites](#1-prerequisites)
2. [Database setup](#2-database-setup)
3. [Application configuration](#3-application-configuration)
4. [Build and run](#4-build-and-run)
5. [Using the API](#5-using-the-api)
6. [Reference](#6-reference)
7. [Running the tests](#7-running-the-tests)
8. [Security notes](#8-security-notes)

---

## 1. Prerequisites

- **.NET SDK 8.0.308 or a later 8.0 feature band.** `global.json` pins `8.0.308` with `rollForward: latestFeature`. Check with `dotnet --version`.
- **SQL Server 2016 or later** (Developer, Express or a full edition) that you can reach from your machine.
  - It must allow **SQL Server authentication** (mixed mode), because the applications connect with the SQL login `demo_app`.
- **`sqlcmd`** or **SQL Server Management Studio (SSMS)** to run the database script.
- **A Google OAuth client** (Google Cloud Console) for signing in to the Web application. See [Web configuration](#web-sourceweb).
- Optional: Visual Studio 2022 (17.8 or later) to open `Demo.sln`.

---

## 2. Database setup

### 2.1 Create the database

`database/CreateDatabase.sql` is safe to run more than once (every statement is guarded by an existence check). It:

- creates the `MarketplaceDB` database if it does not exist;
- creates the server login `demo_app` and the database user `demo_user`, and adds `demo_user` to the `db_datareader` and `db_datawriter` roles (no schema-change rights);
- creates the schemas `catalog`, `orders`, `finance`, `security`, `audit`, `marketing` and `analytics`;
- creates the tables, indexes and constraints for identity (`security`), catalog, orders and audit data.

**Before you run it**, open the script and replace the password in the `CREATE LOGIN [demo_app]` statement with one of your own. Use the same password in the connection string in [section 3](#3-application-configuration). See [Security notes](#8-security-notes).

**With SSMS**

1. Connect to your SQL Server instance.
2. Open `database/CreateDatabase.sql`.
3. Run it (**Execute** or `F5`).

**With `sqlcmd`** (from the repository root)

```powershell
sqlcmd -S <server> -E -i database/CreateDatabase.sql
```

- Replace `<server>` with your instance, for example `localhost` or `.\SQLEXPRESS`.
- `-E` uses your Windows account. For SQL authentication use `-U <user> -P <password>` instead.
- If `sqlcmd` reports a certificate trust error, add `-C`.

### 2.2 Verify

```sql
USE MarketplaceDB;

SELECT s.name AS [schema], COUNT(t.object_id) AS [tables]
FROM sys.schemas AS s
LEFT JOIN sys.tables AS t ON t.schema_id = s.schema_id
WHERE s.name IN ('analytics', 'audit', 'catalog', 'finance', 'marketing', 'orders', 'security')
GROUP BY s.name
ORDER BY s.name;
```

All seven schemas should be listed, and `security` and `catalog` should have tables.

### 2.3 What happens at application start

When the Web application starts it also tries to:

1. run the Entity Framework migration for the Identity context (`source/Infrastructure/Migrations`, one migration: `InitialCreate`);
2. seed the roles, fee categories and product categories (inserts only).

### 2.4 Reset the database

`database/DropDatabase.sql` drops the tables, schemas, `demo_user` and the `demo_app` login. It does **not** drop the `MarketplaceDB` database itself (that line is commented out). To start again, run `DropDatabase.sql` and then `CreateDatabase.sql`.

---

## 3. Application configuration

Secrets are not stored in `appsettings.json`; the committed files leave them blank. Use [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for local development. Both projects already have a `UserSecretsId`, so `dotnet user-secrets init` is not needed.

Run these commands from the repository root. In the connection string, replace `<server>` and `<demo_app-password>`.

### Web (`source/Web`)

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<server>;Database=MarketplaceDB;User Id=demo_app;Password=<demo_app-password>;TrustServerCertificate=True;" --project source/Web
dotnet user-secrets set "Authentication:Google:ClientId" "<google-client-id>" --project source/Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "<google-client-secret>" --project source/Web
```

- Create an OAuth 2.0 **Web application** client in the [Google Cloud Console](https://console.cloud.google.com/).
- Add this **authorised redirect URI**: `https://localhost:7288/signin-google` (the port of the `https` launch profile; the callback path is `/signin-google`).
- The Google credentials are required. The application is not usable with them left blank.

### API (`source/Api`)

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<server>;Database=MarketplaceDB;User Id=demo_app;Password=<demo_app-password>;TrustServerCertificate=True;" --project source/Api
dotnet user-secrets set "Jwt:Key" "<signing-key>" --project source/Api
dotnet user-secrets set "Jwt:Issuer" "<issuer>" --project source/Api
dotnet user-secrets set "Jwt:Audience" "<audience>" --project source/Api
```

- `Jwt:Issuer` and `Jwt:Audience` can be any consistent strings (for example `OnlineMarketplace`). They are used both when a token is issued and when it is validated.
- `Jwt:Key` is the HMAC-SHA256 signing key. Use a random value of at least 32 characters. One way to generate it in PowerShell:

  ```powershell
  $bytes = New-Object byte[] 32
  [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
  [Convert]::ToBase64String($bytes)
  ```

- Optional: allow a browser origin to call the API (CORS). The list is empty by default.

  ```powershell
  dotnet user-secrets set "Cors:AllowedOrigins:0" "https://localhost:7288" --project source/Api
  ```

### Environment variables (alternative)

Any setting can instead be supplied as an environment variable, using `__` for the `:` separator, for example `ConnectionStrings__DefaultConnection`, `Authentication__Google__ClientId` or `Jwt__Key`. Use this for deployments.

### Environments

The launch profiles run in `Development`. For any other environment set `ASPNETCORE_ENVIRONMENT`. `source/Web/appsettings.Production.json` contains a placeholder `AllowedHosts` (`yourdomain.com;www.yourdomain.com`) that you must change before hosting the Web application. Outside `Development` the Web application also forces secure cookies and HTTPS.

---

## 4. Build and run

Trust the ASP.NET Core development certificate once (needed for the `https` profiles):

```powershell
dotnet dev-certs https --trust
```

Build the solution:

```powershell
dotnet restore Demo.sln
dotnet build Demo.sln
```

Run the projects from the repository root, each in its own terminal:

```powershell
dotnet run --project source/Web --launch-profile https
dotnet run --project source/Api --launch-profile https
```

| Project | URLs (launch profiles) | Opens |
|---|---|---|
| Web | `https://localhost:7288` (`https` profile), `http://localhost:7288` (`http` profile) | `/Public/Home/Index` |
| API | `https://localhost:7202` and `http://localhost:5259` (`https` profile), `http://localhost:5259` (`http` profile) | `/swagger` |

- Use the Web `https` profile. The `http` profile has no HTTPS port, and Google sign-in is configured for the HTTPS redirect URI.
- Both projects can run at the same time. They use different ports.
- Both expose a health check at `/health`, which includes a SQL Server check.
- Swagger UI is available only when the API runs in `Development`.
- **Visual Studio:** open `Demo.sln`, choose **Configure Startup Projects**, select **Multiple startup projects**, and set `Web` and `Api` to **Start**.

Sign in to the Web application from `/Identity/Anonymous/Login` (the sign-in page, which hands off to Google).

---

## 5. Using the API

1. **Create an account.** Sign in to the Web application with Google. Your account is created on first sign-in.
2. **Generate an API key.** In the Web application open **Account**, choose the **API** card, and select **Regenerate API Key**. The key is shown once; copy it immediately.
3. **Get a token.** The token is valid for 8 hours.

   ```http
   POST https://localhost:7202/identity/token/login
   Content-Type: application/json

   {
     "email": "you@example.com",
     "apiKey": "<your-api-key>"
   }
   ```

   Response:

   ```json
   {
     "accessToken": "eyJhbGci...",
     "expiresAt": "2026-01-01T12:00:00Z"
   }
   ```

4. **Call a protected endpoint** with the token. The account must have the `Seller` or `Buyer` role.

   ```http
   GET https://localhost:7202/private/product/search?q=widget&page=1&pageSize=24
   Authorization: Bearer <accessToken>
   ```

   Query parameters: `q`, `category`, `minPrice`, `maxPrice`, `condition`, `sortBy` (default `relevance`), `page` (default `1`) and `pageSize` (1 to 100, default `24`).

In Swagger UI, use **Authorize** and paste the access token to try the endpoints.

---

## 6. Reference

### API endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/identity/token/login` | None | Exchange email and API key for a JWT |
| `GET` | `/private/product/search` | Bearer (`Seller` or `Buyer`) | Search products |
| `GET` | `/health` | None | Health check |
| `GET` | `/swagger` | None (development only) | Swagger UI |

### Rate limits

Exceeded limits return `429 Too Many Requests`.

| Scope | Limit |
|---|---|
| API, all endpoints | 100 requests per minute per user, or per IP address when anonymous |
| API, `POST /identity/token/login` | 10 requests per minute |
| Web, `POST` sign-in (`Identity/Anonymous/Login`) | 10 requests per minute |

### Logs

Both projects log to the console and to daily rolling files kept for 30 days. The `logs` folder is created relative to the process working directory, so with `dotnet run` from the repository root it appears in the repository root.

| Project | File |
|---|---|
| Web | `logs/app-<date>.log` |
| API | `logs/api-<date>.log` |

---

## 7. Running the tests

```powershell
dotnet test Demo.sln
```

This runs the xUnit and Moq tests in `test/Unit`. They do not need a database or any configuration.

Run a subset:

```powershell
dotnet test test/Unit --filter "FullyQualifiedName~Security"
```

---

## 8. Security notes

- **Change the `demo_app` password.** `database/CreateDatabase.sql` contains a fixed password for the login. For anything other than a throwaway local database, edit it before running the script, or change it afterwards:

  ```sql
  ALTER LOGIN [demo_app] WITH PASSWORD = '<new-strong-password>';
  ```

  Then update the connection strings in your secrets.
- **Never commit secrets.** Keep connection strings, Google credentials and `Jwt:Key` in User Secrets or environment variables, not in `appsettings*.json`.
- **Keep `Jwt:Key` private and long.** Anyone with the key can mint valid tokens. Rotate it by changing the value and restarting the API; existing tokens then stop working.
- **Treat API keys like passwords.** They are shown once and cannot be retrieved later; regenerate one if it may have been exposed.
- **`TrustServerCertificate=True`** in the examples is for local development. For a real server, use a trusted certificate and remove it.
