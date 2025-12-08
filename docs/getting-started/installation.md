# 📦 Instalación

Guía completa para instalar todas las herramientas necesarias para desarrollar en el proyecto.

## Requisitos del Sistema

- **macOS** (versión actual soportada)
- Conexión a internet
- Permisos de administrador (para algunas instalaciones)

---

## Herramientas Necesarias

1. [Node.js y npm](#instalación-de-nodejs)
2. [.NET 8 SDK](#instalación-de-net-8-sdk)
3. [Azure CLI](#instalación-de-azure-cli) (opcional, para deployment)

---

## Instalación de Node.js

### Opción 1: Descarga Directa (Recomendado)

1. Visita: https://nodejs.org/
2. Descarga la versión **LTS** (18.x o superior)
3. Ejecuta el instalador
4. Verifica la instalación:
   ```bash
   node --version
   npm --version
   ```

### Opción 2: Usando Homebrew

```bash
brew install node
```

---

## Instalación de .NET 8 SDK

### Opción 1: Descarga Directa (Más Fácil) ⭐

1. **Visita**: https://dotnet.microsoft.com/download/dotnet/8.0
2. **Descarga**: Click "Download .NET SDK 8.0.x" para macOS
3. **Instala**: 
   - Abre el archivo `.pkg` descargado
   - Sigue el asistente de instalación
   - Se instalará en `/usr/local/share/dotnet`
4. **Verifica**: Abre una **nueva terminal** y ejecuta:
   ```bash
   dotnet --version
   ```
   Debería mostrar: `8.0.x`

### Opción 2: Usando Homebrew

```bash
# Instala Homebrew primero (si no lo tienes)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Luego instala .NET
brew install --cask dotnet-sdk
```

### Opción 3: Script de Instalación

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
```

### Verificar Instalación

```bash
dotnet --version      # Debería mostrar 8.0.x
dotnet --list-sdks    # Muestra SDKs instalados
```

### Troubleshooting: "dotnet: command not found"

Si después de la instalación no encuentras el comando `dotnet`:

1. **Abre una nueva terminal** (para refrescar PATH)

2. Si aún no funciona, agrega a `~/.zshrc`:
   ```bash
   export PATH="$PATH:/usr/local/share/dotnet"
   export DOTNET_ROOT="/usr/local/share/dotnet"
   ```

3. Recarga la configuración:
   ```bash
   source ~/.zshrc
   ```

4. Verifica las ubicaciones comunes:
   ```bash
   # Verificar si está instalado pero no en PATH
   ls -la /usr/local/share/dotnet/dotnet
   ls -la ~/.dotnet/dotnet
   ```

---

## Instalación de Azure CLI

Azure CLI es necesario para deployment y gestión de recursos en Azure.

### Opción 1: Usando Homebrew (Recomendado)

```bash
brew update && brew install azure-cli
```

### Opción 2: Script de Instalación

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### Opción 3: Instalador de macOS

1. Descarga el instalador: https://aka.ms/installazureclimacOS
2. Ejecuta el archivo `.pkg`
3. Sigue el asistente de instalación

### Verificar Instalación

```bash
az --version
```

Debería mostrar la versión de Azure CLI instalada (2.40 o superior recomendado).

### Configurar Azure CLI

Después de instalar, configura tu cuenta:

```bash
# Login en Azure
az login

# Verificar suscripción activa
az account show

# Listar suscripciones disponibles
az account list

# Establecer suscripción por defecto
az account set --subscription "<subscription-id>"
```

---

## Verificación Completa

Ejecuta estos comandos para verificar que todo está instalado correctamente:

```bash
# Verificar Node.js
node --version      # Debería mostrar v18.x.x o superior
npm --version       # Debería mostrar 9.x.x o superior

# Verificar .NET
dotnet --version    # Debería mostrar 8.0.x

# Verificar Azure CLI (opcional)
az --version        # Debería mostrar 2.40.x o superior
```

---

## Próximos Pasos

Una vez completada la instalación:

1. ✅ **Configura el entorno local**: Ve a [Desarrollo Local](./local-development.md)
2. ✅ **Instala dependencias del proyecto**: 
   ```bash
   # Backend
   cd backend/VendorMdm.Api
   dotnet restore
   
   # Frontend
   cd frontend
   npm install
   ```

---

## Troubleshooting

### Problemas Comunes

#### .NET no se encuentra después de la instalación
- Abre una **nueva terminal** después de instalar
- Agrega `.dotnet` al PATH en `~/.zshrc`
- Verifica la ubicación de instalación

#### Node.js versión incorrecta
- Usa Node Version Manager (nvm) para gestionar versiones:
  ```bash
  brew install nvm
  nvm install 18
  nvm use 18
  ```

#### Azure CLI no puede conectarse
- Verifica tu conexión a internet
- Intenta `az login --use-device-code` si hay problemas con el navegador

---

## Recursos Adicionales

- [Documentación oficial de .NET](https://docs.microsoft.com/dotnet/)
- [Documentación de Node.js](https://nodejs.org/docs/)
- [Documentación de Azure CLI](https://docs.microsoft.com/cli/azure/)

---

*¿Tienes problemas con la instalación? Consulta la sección de [Troubleshooting](../../troubleshooting/README.md) o revisa los problemas comunes arriba.*

