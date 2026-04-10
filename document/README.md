# DemoLite Setup Guide

## Prerequisites

- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (2016 or later)
- [.NET SDK 8.0.308](https://dotnet.microsoft.com/download/dotnet/8.0) or later

---

## Project Overview

This solution contains two runnable projects:

| Project | Purpose | Auth |
|---|---|---|
| `source/Web` | Razor Pages web application | Google OAuth (cookie) |
| `source/Api` | REST API | JWT Bearer token |

Both projects share the same SQL Server database (`MarketplaceDB`) and the same Infrastructure layer.

---

## 1. Create the Database

Run the `CreateDatabase.sql` script against your SQL Server instance. This will:

- Create the `MarketplaceDB` database (if it does not already exist)
- Create the `demo_app` login and `demo_user` database user
- Apply the full schema including tables, indexes, and constraints

**Using SQL Server Management Studio (SSMS):**

1. Open SSMS and connect to your SQL Server instance.
2. Open the file `database/CreateDatabase.sql`.
3. Click **Execute** (or press `F5`).

**Using the command line (sqlcmd):**

```bash
sqlcmd -S <server> -E -i database/CreateDatabase.sql
```

Replace `<server>` with your SQL Server instance name (e.g. `localhost` or `.\SQLEXPRESS`).

---

## 2. Configure the Web Project

Sensitive values are stored using [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets). Do **not** place secrets in `appsettings.json`.

From the repository root, initialise and set secrets for the Web project:

```bash
dotnet user-secrets init --project source/Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<server>;Database=MarketplaceDB;User Id=demo_app;Password=7#K@2XwR!nTz*8LjV&bF4sHc;TrustServerCertificate=True;" --project source/Web
dotnet user-secrets set "Authentication:Google:ClientId" "<your-google-client-id>" --project source/Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "<your-google-client-secret>" --project source/Web
```

> Google OAuth credentials can be obtained from the [Google Cloud Console](https://console.cloud.google.com/). Add `https://localhost:<port>/signin-google` as an authorised redirect URI.

---

## 3. Configure the API Project

The API uses JWT Bearer authentication. Set secrets for the API project:

```bash
dotnet user-secrets init --project source/Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<server>;Database=MarketplaceDB;User Id=demo_app;Password=7#K@2XwR!nTz*8LjV&bF4sHc;TrustServerCertificate=True;" --project source/Api
dotnet user-secrets set "Jwt:Key" "<your-jwt-signing-key>" --project source/Api
dotnet user-secrets set "Jwt:Issuer" "<your-issuer>" --project source/Api
dotnet user-secrets set "Jwt:Audience" "<your-audience>" --project source/Api
```

Optionally, configure allowed CORS origins (required if calling the API from a browser):

```bash
dotnet user-secrets set "Cors:AllowedOrigins:0" "https://localhost:<web-port>" --project source/Api
```

**JWT Key guidance:**
- Use a cryptographically random string of at least 32 characters.
- Example (PowerShell): `[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])`

---

## 4. Run the Applications

**Web project:**

```bash
dotnet run --project source/Web
```

**API project:**

```bash
dotnet run --project source/Api
```

Both projects can run simultaneously. The URLs will be shown in console output (e.g. `https://localhost:7000` for Web, `https://localhost:7001` for API).

> On first run, the Web project automatically applies any pending Entity Framework migrations and seeds required roles.

---

## 5. Using the API

### Step 1 — Create an account

Sign in to the **Web application** using Google OAuth. On first sign-in your account is provisioned automatically.

### Step 2 — Retrieve your API key

Navigate to your **Profile** page in the Web application. Use the **Regenerate API Key** button to generate a key. Copy it immediately — it is shown only once.

### Step 3 — Obtain a JWT token

```http
POST /identity/token/login
Content-Type: application/json

{
  "email": "you@example.com",
  "apiKey": "<your-api-key>"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGci...",
  "expiresAt": "2026-04-10T05:00:00Z"
}
```

Tokens are valid for **8 hours**.

### Step 4 — Call protected endpoints

Include the token in the `Authorization` header:

```
Authorization: Bearer <accessToken>
```

---

## 6. API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/identity/token/login` | None | Exchange email + API key for a JWT |
| `GET` | `/private/product/search` | Bearer | Search products |
| `GET` | `/health` | None | Health check |
| `GET` | `/swagger` | None (dev only) | Swagger UI |

---

## 7. Rate Limiting

| Scope | Limit |
|---|---|
| All API endpoints (global) | 100 requests per minute per user/IP |
| `POST /identity/token/login` | 10 requests per minute per user/IP |

Exceeded limits return `429 Too Many Requests`.

---

## 8. Logs

Both projects write structured logs to rolling daily files:

| Project | Log path |
|---|---|
| Web | `source/Web/logs/app-<date>.log` |
| API | `source/Api/logs/api-<date>.log` |

Logs are retained for 30 days.

---

## 9. Running Tests

```bash
dotnet test
```

Unit tests are located in `test/Unit/`.
