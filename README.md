# Talanton-Trust-Engine
SACCO lending platform for loan applications, underwriting and committee decision-making.

## Connect to a New Supabase Project

The backend now expects the database connection string from environment variables so old project values are not reused.

### 1) Get your new Supabase DB connection string

From your Supabase dashboard:
- Open your new project.
- Go to `Project Settings` > `Database`.
- Copy the Postgres connection string.

### 2) Set environment variable (PowerShell)

For the current terminal session:

```powershell
$env:SUPABASE_DB_CONNECTION="Host=<your-host>;Port=5432;Database=postgres;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
```

Persist for your user account (new terminals):

```powershell
[System.Environment]::SetEnvironmentVariable("SUPABASE_DB_CONNECTION", "Host=<your-host>;Port=5432;Database=postgres;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true", "User")
```

### 3) Run backend

```powershell
cd Backend/Talanton.Api
dotnet run
```

If you use EF Core migrations, run them after switching projects:

```powershell
dotnet ef database update
```
