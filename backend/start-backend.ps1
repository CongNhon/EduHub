param(
    [switch]$ApplyMigrations,
    [switch]$InitializeOnly,
    [switch]$SeedData
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$envFile = Join-Path $root ".env"
$project = Join-Path $root "src\EduHub.WebApi\EduHub.WebApi.csproj"
$apiPort = 8080

if (-not (Test-Path -LiteralPath $envFile)) {
    throw "Missing .env at $envFile"
}

foreach ($line in Get-Content -LiteralPath $envFile) {
    $trimmed = $line.Trim()
    if (-not $trimmed -or $trimmed.StartsWith("#")) {
        continue
    }

    $separator = $trimmed.IndexOf("=")
    if ($separator -lt 1) {
        continue
    }

    $name = $trimmed.Substring(0, $separator).Trim()
    $value = $trimmed.Substring($separator + 1).Trim()
    if ($value.Length -ge 2 -and (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'")))) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    [Environment]::SetEnvironmentVariable($name, $value, "Process")
}

foreach ($requiredName in "ConnectionStrings__Postgres", "ConnectionStrings__Mongo") {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($requiredName, "Process"))) {
        throw "Missing required setting: $requiredName"
    }
}

$listener = Get-NetTCPConnection -State Listen -LocalPort $apiPort -ErrorAction SilentlyContinue | Select-Object -First 1
if ($listener) {
    $process = Get-CimInstance Win32_Process -Filter "ProcessId=$($listener.OwningProcess)" -ErrorAction SilentlyContinue
    throw "Port $apiPort is already used by PID $($listener.OwningProcess): $($process.Name). Stop Docker API or the previous local API first."
}

$redisPort = if ($env:REDIS_HOST_PORT) { $env:REDIS_HOST_PORT } else { "6379" }
$redisPassword = if ($env:REDIS_PASSWORD) { $env:REDIS_PASSWORD } else { $env:POSTGRES_PASSWORD }
if ([string]::IsNullOrWhiteSpace($redisPassword)) {
    throw "Missing REDIS_PASSWORD or POSTGRES_PASSWORD in .env"
}

Push-Location $root
try {
    $usesLocalPostgres = $env:ConnectionStrings__Postgres -match "(?i)(Host|Server)\s*=\s*(localhost|127\.0\.0\.1)"
    $usesLocalMongo = $env:ConnectionStrings__Mongo -match "(?i)mongodb(?:\+srv)?://(?:[^@/]+@)?(localhost|127\.0\.0\.1)"
    $composeArguments = @("compose")
    $composeArguments += @("up", "-d", "--wait", "redis")
    if ($usesLocalPostgres) { $composeArguments += "postgres" }
    if ($usesLocalMongo) { $composeArguments += "mongodb" }

    & docker @composeArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Cannot start local dependencies. Start Docker Desktop and retry."
    }

    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://localhost:$apiPort"
    $env:ConnectionStrings__Redis = "localhost:$redisPort,password=$redisPassword"
    $env:Reports__StoragePath = Join-Path $root ".local\reports"
    $env:EvidenceStorage__LocalRootPath = Join-Path $root ".local\profile-evidence"

    New-Item -ItemType Directory -Force -Path $env:Reports__StoragePath | Out-Null
    New-Item -ItemType Directory -Force -Path $env:EvidenceStorage__LocalRootPath | Out-Null

    if ($usesLocalPostgres -or $ApplyMigrations -or $InitializeOnly) {
        $env:EDUHUB_POSTGRES_CONNECTION = $env:ConnectionStrings__Postgres
        dotnet tool restore
        if ($LASTEXITCODE -ne 0) { throw "Cannot restore local .NET tools." }
        dotnet tool run dotnet-ef database update --project .\src\EduHub.Infrastructure\EduHub.Infrastructure.csproj --startup-project .\src\EduHub.WebApi\EduHub.WebApi.csproj --context ApplicationDbContext
        if ($LASTEXITCODE -ne 0) { throw "Cannot apply database migrations." }
    }

    if ($SeedData) {
        dotnet run --project .\tools\EduHub.DatabaseManager\EduHub.DatabaseManager.csproj -- seed
        if ($LASTEXITCODE -ne 0) { throw "Cannot seed development data." }
    }

    if ($InitializeOnly) {
        Write-Host "Local dependencies and database migrations are ready."
        return
    }

    dotnet watch --project $project --launch-profile http
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
