param([string]$SqlServer = 'localhost\SQLEXPRESS')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $projectRoot 'CoffeManager.csproj'
$builder = [System.Data.Common.DbConnectionStringBuilder]::new()
$builder['Server'] = $SqlServer
$builder['Database'] = 'CoffeManager'
$builder['Integrated Security'] = 'True'
$builder['Encrypt'] = 'True'
$builder['TrustServerCertificate'] = 'True'
$randomBytes = New-Object byte[] 32
$generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try { $generator.GetBytes($randomBytes) } finally { $generator.Dispose() }
$activationKey = [BitConverter]::ToString($randomBytes).Replace('-', '')
dotnet user-secrets set 'ConnectionStrings:CoffeManager' $builder.ConnectionString --project $project
if ($LASTEXITCODE -ne 0) { throw 'No se pudo configurar la conexión.' }
dotnet user-secrets set 'Setup:Key' $activationKey --project $project
if ($LASTEXITCODE -ne 0) { throw 'No se pudo configurar la activación.' }
dotnet dev-certs https --trust
if ($LASTEXITCODE -ne 0) { throw 'No se pudo preparar el certificado HTTPS.' }
Write-Host 'Clave para ACTIVAR LA PRIMERA CUENTA en el cliente:'
Write-Host $activationKey
Write-Host 'La clave se configuró para desarrollo. No la publiques. Una cuenta existente no puede recrearse con esta clave.'
Write-Host 'Ejecuta ahora desde la carpeta del proyecto:'
Write-Host 'dotnet run --project CoffeManager.csproj --launch-profile https -- --initialize-database'
Write-Host 'La base CoffeManager debe existir antes. El script no crea ni borra tu base automáticamente.'
