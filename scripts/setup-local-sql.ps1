$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot '../Pp.Api/Pp.Api.csproj'
if (-not (Test-Path -LiteralPath $project)) {
    throw 'Pp.Api projesi bulunamadı. Bu dosyayı deponun scripts klasöründen çalıştır.'
}
$project = (Resolve-Path -LiteralPath $project).Path

docker info --format '{{.ServerVersion}}'
if ($LASTEXITCODE -ne 0) { throw 'Docker Desktop açık olmalı ve terminal Docker erişimine sahip olmalı.' }

$container = 'papara-sql'
$existing = docker ps -a --filter "name=^/$container$" --format '{{.Names}}'
if ($LASTEXITCODE -ne 0) { throw 'Docker konteynerleri listelenemedi.' }

try {
    if ($existing -eq $container) {
        $bindingsJson = docker inspect --format '{{json .HostConfig.PortBindings}}' $container
        if ($LASTEXITCODE -ne 0) { throw 'Mevcut SQL Server ayarları okunamadı.' }
        $bindings = ($bindingsJson | ConvertFrom-Json).'1433/tcp'
        if (@($bindings).Count -ne 1 -or $bindings.HostIp -ne '127.0.0.1' -or $bindings.HostPort -ne '14333') {
            throw 'Mevcut papara-sql konteyneri beklenen yerel porta bağlı değil.'
        }
        docker start $container
        if ($LASTEXITCODE -ne 0) { throw 'Mevcut SQL Server başlatılamadı.' }
        $containerEnvironment = docker inspect --format '{{range .Config.Env}}{{println .}}{{end}}' $container
        if ($LASTEXITCODE -ne 0) { throw 'Konteynerin kurulum ayarları okunamadı.' }
        $passwordEntry = $containerEnvironment | Where-Object { $_ -like 'MSSQL_SA_PASSWORD=*' } | Select-Object -First 1
        $password = if ($passwordEntry) { $passwordEntry.Substring('MSSQL_SA_PASSWORD='.Length) }
        if ([string]::IsNullOrWhiteSpace($password)) {
            throw 'Mevcut konteynerde SQL Server kurulum parolası bulunamadı.'
        }
    } else {
        $randomBytes = New-Object byte[] 24
        $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try { $random.GetBytes($randomBytes) } finally { $random.Dispose() }
        $password = 'Aa1!' + [Convert]::ToBase64String($randomBytes)
        $env:MSSQL_SA_PASSWORD = $password
        docker run -d --name $container -e ACCEPT_EULA=Y -e MSSQL_PID=Developer -e MSSQL_SA_PASSWORD -p 127.0.0.1:14333:1433 -v papara-sql-data:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-latest
        if ($LASTEXITCODE -ne 0) { throw 'SQL Server konteyneri başlatılamadı.' }
    }

    $env:SQLCMDPASSWORD = $password
    $ready = $false
    $deadline = (Get-Date).AddMinutes(3)
    Write-Host 'SQL Server bağlantı kabul edene kadar bekleniyor.'
    while ((Get-Date) -lt $deadline) {
        $probe = & {
            # Windows PowerShell treats native stderr as an error record.
            $ErrorActionPreference = 'Continue'
            $output = docker exec -e SQLCMDPASSWORD $container /opt/mssql-tools18/bin/sqlcmd -S tcp:127.0.0.1,1433 -d master -U sa -C -b -l 5 -h -1 -Q "SET NOCOUNT ON; SELECT 'PAPARA_SQL_READY'" 2>&1
            [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
        }
        if ($probe.ExitCode -eq 0 -and ($probe.Output -match '^\s*PAPARA_SQL_READY\s*$')) {
            $ready = $true
            break
        }
        $status = docker inspect --format '{{.State.Status}}' $container
        if ($LASTEXITCODE -ne 0) { throw 'SQL Server konteynerinin durumu okunamadı.' }
        if ($status -notin @('running', 'restarting')) {
            docker logs --tail 20 $container
            throw "SQL Server konteyneri bağlantı açmadan durdu. Durum: $status"
        }
        Start-Sleep -Seconds 3
    }
    if (-not $ready) {
        $probe.Output | ForEach-Object { Write-Host $_ }
        docker logs --tail 20 $container
        throw 'SQL Server üç dakika içinde bağlantı kabul etmedi.'
    }

    docker exec -e SQLCMDPASSWORD $container /opt/mssql-tools18/bin/sqlcmd -S localhost -d master -U sa -C -b -Q "IF DB_ID(N'PaparaFinalCase') IS NULL CREATE DATABASE [PaparaFinalCase]"
    if ($LASTEXITCODE -ne 0) { throw 'PaparaFinalCase veritabanı oluşturulamadı.' }

    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection['Server'] = 'tcp:127.0.0.1,14333'
    $connection['Database'] = 'PaparaFinalCase'
    $connection['User ID'] = 'sa'
    $connection['Password'] = $password
    $connection['Encrypt'] = 'True'
    $connection['TrustServerCertificate'] = 'True'
    $connection['Connect Timeout'] = '8'

    Write-Host 'Konteynerde SQL girişi doğrulandı. .NET veritabanı geçişleri uygulanıyor.'
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ConnectionStrings__MsSqlConnection = $connection.ConnectionString
    dotnet run --no-launch-profile --project $project -- --migrate
    if ($LASTEXITCODE -ne 0) { throw 'Veritabanı geçişleri uygulanamadı.' }
    dotnet user-secrets set 'ConnectionStrings:MsSqlConnection' $connection.ConnectionString --project $project
    if ($LASTEXITCODE -ne 0) { throw 'Bağlantı ayarı kaydedilemedi.' }
    Write-Host 'PaparaFinalCase hazır. API projesini Visual Studio üzerinden başlatabilirsin.'
} finally {
    Remove-Item Env:MSSQL_SA_PASSWORD, Env:SQLCMDPASSWORD, Env:ConnectionStrings__MsSqlConnection -ErrorAction SilentlyContinue
}
