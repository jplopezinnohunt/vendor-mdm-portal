# 💻 Desarrollo Local

Guía completa para configurar y ejecutar el proyecto localmente.

## Inicio Rápido (5 minutos)

### 1. Instalar Dependencias

```bash
# Backend
cd backend/VendorMdm.Api
dotnet restore

# Frontend
cd frontend
npm install
```

### 2. Ejecutar el Proyecto

**Terminal 1 - Backend:**
```bash
cd backend/VendorMdm.Api
dotnet run
```
El backend estará disponible en: http://localhost:5001

**Terminal 2 - Frontend:**
```bash
cd frontend
npm run dev
```
El frontend estará disponible en: http://localhost:5173

### 3. Acceder a la Aplicación

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger

---

## Configuración Detallada

### Configuración del Backend

El backend está configurado para usar emuladores locales por defecto.

#### Opción 1: Desarrollo con Emuladores Locales

No se requiere configuración adicional. El backend usará:
- SQLite para la base de datos
- Logging en consola para emails
- Emuladores locales si están instalados

#### Opción 2: Conectar con Recursos de Azure

Para usar recursos de Azure en desarrollo local:

1. **Obtener Connection Strings** desde Azure Portal
2. **Configurar User Secrets** (recomendado):
   ```bash
   cd backend/VendorMdm.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Sql" "YOUR_CONNECTION_STRING"
   dotnet user-secrets set "ConnectionStrings:Cosmos" "YOUR_CONNECTION_STRING"
   dotnet user-secrets set "ConnectionStrings:ServiceBus" "YOUR_CONNECTION_STRING"
   ```

3. O editar `appsettings.Development.json` (no se commitea a git)

Para más detalles, consulta: [Conexión Local con Azure](../../azure/local-azure-setup.md)

---

### Configuración del Frontend

El frontend está configurado para conectarse al backend local.

#### Variables de Entorno (Opcional)

Crea un archivo `.env.local` en la raíz del proyecto:

```env
VITE_API_BASE_URL=http://localhost:5001
VITE_ENVIRONMENT=development
```

**Nota**: El frontend ya está configurado para usar `http://localhost:5001` por defecto.

---

## Estructura del Proyecto

```
vendor-mdm-portal/
├── backend/
│   ├── VendorMdm.Api/          # API principal (.NET 8)
│   ├── VendorMdm.Artifacts/    # Azure Functions
│   └── VendorMdm.Shared/       # Modelos compartidos
├── frontend/                    # React + TypeScript + Vite
├── infrastructure/              # Templates Bicep para Azure
└── docs/                        # Documentación
```

---

## Configuración de Puertos

### Backend
- **HTTP**: http://localhost:5001
- **HTTPS**: https://localhost:5001 (desarrollo)
- **Swagger**: http://localhost:5001/swagger

**Nota**: El puerto 5000 está reservado por AirPlay en macOS, por lo que usamos 5001.

### Frontend
- **Development Server**: http://localhost:5173
- **Build Output**: `frontend/dist/`

---

## Base de Datos Local

### SQLite (Por Defecto)

El backend usa SQLite por defecto en desarrollo local:
- Archivo: `backend/VendorMdm.Api/vendormdm.db`
- Se crea automáticamente al ejecutar migrations

### Migraciones de Base de Datos

```bash
cd backend/VendorMdm.Api

# Crear una nueva migración
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones
dotnet ef database update
```

---

## Autenticación en Desarrollo

El proyecto incluye autenticación mock para desarrollo local:

### Roles Disponibles

1. **Vendor User** - Click "Access Portal"
2. **Approver User** - Click "Log in as Approver"
3. **Admin User** - Click "Log in as Administrator"

### Rutas por Rol

- **Vendor**: `/profile`, `/dashboard`, `/requests`
- **Approver**: `/approver/worklist`
- **Admin**: `/admin/dashboard`, `/admin/invite-vendor`

---

## Envío de Emails en Desarrollo

Por defecto, los emails se **registran en consola** y no se envían realmente.

### Configurar SMTP (Opcional)

Para enviar emails reales en desarrollo:

1. Edita `backend/VendorMdm.Api/appsettings.Development.json`
2. Configura SMTP (ver [Configuración de Email](../../features/email-configuration.md))
3. Reinicia el backend

---

## Testing

### Backend (API)

```bash
cd backend/VendorMdm.Api

# Ejecutar tests (cuando estén disponibles)
dotnet test
```

### Frontend

```bash
cd frontend

# Ejecutar tests
npm test

# Ejecutar tests en modo watch
npm run test:watch
```

Para más detalles: [Guía de Testing](../../guides/testing.md)

---

## Troubleshooting

### Backend no inicia

**Error**: "Port 5001 already in use"
```bash
# Encontrar y cerrar el proceso
lsof -ti:5001 | xargs kill
```

**Error**: "Cannot find module"
```bash
cd backend/VendorMdm.Api
dotnet restore
```

### Frontend no conecta al backend

**Error**: "Cannot connect to backend API"
- Verifica que el backend esté corriendo en el puerto 5001
- Verifica la consola del navegador (F12) para errores detallados
- Verifica CORS en `Program.cs`

### Problemas de base de datos

**Error**: "Table does not exist"
```bash
cd backend/VendorMdm.Api
dotnet ef database update
```

---

## Scripts Útiles

### Backend

```bash
# Iniciar backend con script helper
./start-backend.sh

# Restaurar dependencias
dotnet restore

# Limpiar build
dotnet clean

# Build en release
dotnet build -c Release
```

### Frontend

```bash
# Instalar dependencias
npm install

# Desarrollo
npm run dev

# Build de producción
npm run build

# Preview de build de producción
npm run preview
```

---

## Flujo de Trabajo Recomendado

1. **Iniciar backend** en una terminal
2. **Iniciar frontend** en otra terminal
3. **Abrir navegador** en http://localhost:5173
4. **Login como Admin** para acceder al dashboard
5. **Probar funcionalidades** según estés desarrollando

---

## Desarrollo con Azure Resources

Si prefieres usar recursos de Azure en lugar de emuladores locales:

👉 Consulta: [Conexión Local con Azure](../../azure/local-azure-setup.md)

Esto incluye:
- Configuración de connection strings
- Firewall rules
- User Secrets
- Verificación de conexión

---

## Próximos Pasos

Una vez configurado el desarrollo local:

1. ✅ Revisa la [Arquitectura del Proyecto](../../architecture/README.md)
2. ✅ Consulta las [Guías de Features](../../features/README.md)
3. ✅ Lee sobre [Deployment en Azure](../../azure/deployment/README.md) cuando estés listo

---

## Referencias Rápidas

- **Puertos**:
  - Frontend: 5173
  - Backend: 5001
  - Swagger: 5001/swagger

- **Archivos importantes**:
  - Backend config: `backend/VendorMdm.Api/appsettings.Development.json`
  - Frontend config: `frontend/vite.config.ts`
  - Database: `backend/VendorMdm.Api/vendormdm.db`

- **Comandos rápidos**:
  ```bash
  # Backend
  cd backend/VendorMdm.Api && dotnet run
  
  # Frontend
  cd frontend && npm run dev
  ```

---

*¿Problemas? Consulta [Troubleshooting](../../troubleshooting/README.md) o revisa los problemas comunes arriba.*

