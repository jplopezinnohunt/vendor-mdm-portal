# Script PowerShell para actualizar secretos de email en Azure Key Vault
# Uso: .\update-keyvault-secrets.ps1 -Environment dev -KeyVaultName vendormdm-kv-dev

param(
    [string]$Environment = "dev",
    [string]$KeyVaultName = "vendormdm-kv-$Environment"
)

Write-Host "🔐 Actualizando secretos en Azure Key Vault: $KeyVaultName" -ForegroundColor Cyan
Write-Host ""

# Verificar que Azure CLI está instalado
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI no está instalado" -ForegroundColor Red
    Write-Host "Instala desde: https://aka.ms/InstallAzureCLI" -ForegroundColor Yellow
    exit 1
}

# Verificar que está logueado
try {
    az account show | Out-Null
} catch {
    Write-Host "⚠️ No estás logueado en Azure CLI" -ForegroundColor Yellow
    Write-Host "Ejecuta: az login" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Conectado a Azure" -ForegroundColor Green
Write-Host ""

# Solicitar credenciales
$SmtpHost = Read-Host "📧 SMTP Host (ej: smtp.gmail.com)"
$SmtpUsername = Read-Host "👤 SMTP Username (ej: user@example.com)"
$SmtpPassword = Read-Host "🔑 SMTP Password (app password)" -AsSecureString
$SmtpPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SmtpPassword)
)
$FromEmail = Read-Host "📨 From Email (ej: noreply@example.com)"
$FromName = Read-Host "📝 From Name (ej: Vendor Management)"
if ([string]::IsNullOrWhiteSpace($FromName)) {
    $FromName = "Vendor Management"
}

Write-Host ""
Write-Host "🔄 Actualizando secretos en Key Vault..." -ForegroundColor Yellow

# Actualizar secretos
az keyvault secret set --vault-name $KeyVaultName --name "EmailService--Smtp--Host" --value $SmtpHost | Out-Null
az keyvault secret set --vault-name $KeyVaultName --name "EmailService--Smtp--Username" --value $SmtpUsername | Out-Null
az keyvault secret set --vault-name $KeyVaultName --name "EmailService--Smtp--Password" --value $SmtpPasswordPlain | Out-Null
az keyvault secret set --vault-name $KeyVaultName --name "EmailService--Smtp--FromEmail" --value $FromEmail | Out-Null
az keyvault secret set --vault-name $KeyVaultName --name "EmailService--Smtp--FromName" --value $FromName | Out-Null

Write-Host ""
Write-Host "✅ Secretos actualizados exitosamente en $KeyVaultName" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Secretos configurados:" -ForegroundColor Cyan
Write-Host "   - EmailService--Smtp--Host"
Write-Host "   - EmailService--Smtp--Username"
Write-Host "   - EmailService--Smtp--Password"
Write-Host "   - EmailService--Smtp--FromEmail"
Write-Host "   - EmailService--Smtp--FromName"
Write-Host ""
Write-Host "💡 Reinicia tu App Service para que los cambios surtan efecto" -ForegroundColor Yellow

