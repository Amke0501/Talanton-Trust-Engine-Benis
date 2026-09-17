# Talanton Trust Engine

Talanton is a SACCO lending platform for member applications, document verification, underwriting, committee quorum decisions, and loan disbursement tracking.

The repository contains:

- `Backend/Talanton.Api`: ASP.NET Core 8 Web API with PostgreSQL/Supabase persistence.
- `Backend/Talanton.Api.Tests`: service and business-rule tests.
- `Frontend-new`: Next.js 16 web application for applicants, underwriters, and committee members.
## Prerequisites

- .NET SDK 8
- Node.js and npm
- A Supabase project or another PostgreSQL database

## Configuration

### Backend database

The API reads the first configured value from `SUPABASE_DB_CONNECTION`, `DATABASE_URL`, or `ConnectionStrings:DefaultConnection`.

For Supabase, use the Npgsql key-value connection string format. In PowerShell, set it for the current terminal session:

```powershell
$env:SUPABASE_DB_CONNECTION="Host=<your-host>;Port=5432;Database=postgres;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
```

Do not commit connection strings, passwords, or private keys. The API applies pending EF Core migrations and seeds demo users when it starts and can reach the database.

### Frontend environment

Create `Frontend-new/.env.local` with values for the Supabase project and API:

```dotenv
NEXT_PUBLIC_SUPABASE_URL=https://<project-ref>.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=<supabase-anon-key>
NEXT_PUBLIC_API_URL=http://localhost:5195
```

The frontend falls back to `http://localhost:5195` when `NEXT_PUBLIC_API_URL` is not set. Use the deployed API URL when running against a hosted backend.

## Run locally

Start the API in one terminal:

```powershell
cd Backend/Talanton.Api
dotnet restore
dotnet run
```

The API exposes a health response at `http://localhost:5195/` and Swagger in Development at `http://localhost:5195/swagger`.

Start the frontend in a second terminal:

```powershell
cd Frontend-new
npm install
npm run dev
```

Open `http://localhost:3000` in a browser.

## Tests and checks

Run backend tests:

```powershell
dotnet test Backend/Talanton.Api.Tests/Talanton.Api.Tests.csproj
```

Run frontend linting and a production build:

```powershell
cd Frontend-new
npm run lint
npm run build
```

## Database migrations

The API applies pending migrations automatically during startup when the database is reachable. To apply them manually:

```powershell
cd Backend/Talanton.Api
dotnet ef database update
```

To add a migration after changing the EF Core model:

```powershell
dotnet ef migrations add <MigrationName>
```

Review generated migrations before applying them to a shared or production database.

## Application flow

1. An applicant creates or resumes a loan application and submits the required information and documents.
2. An underwriter verifies documents, evaluates guardrails, records qualitative audits, and signs off.
3. The committee reviews the frozen underwriting snapshot and votes on the application.
4. A successful quorum enables disbursement and moves the loan into the active portfolio.

## Security notes

- Public self-registration is disabled; accounts are provisioned by SACCO management.
- Never place database passwords or service-role Supabase keys in frontend environment variables.
- Only values prefixed with `NEXT_PUBLIC_` should be exposed to the browser.
- Configure `CORS_ALLOWED_ORIGINS` or `CORS_ALLOWED_ORIGIN_PATTERNS` for non-default frontend deployments.
```

