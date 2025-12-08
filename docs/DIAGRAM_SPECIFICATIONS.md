# 📐 Especificaciones para Diagramas Profesionales con Iconos de Azure

Este documento contiene las especificaciones detalladas para crear diagramas profesionales del sistema, listos para ser implementados en herramientas como Draw.io, Visio, o por un diseñador profesional.

---

## ⚠️ Nota Importante

Los asistentes de IA como yo no tenemos capacidad de generar imágenes directamente. Sin embargo, puedo:

- ✅ Proporcionar especificaciones detalladas
- ✅ Guiarte para usar herramientas profesionales
- ✅ Crear código para diagramas (Mermaid, PlantUML, etc.)
- ✅ Describir exactamente qué debe contener cada diagrama

Para crear diagramas con iconos oficiales de Microsoft Azure, necesitas usar:
1. **Draw.io** (gratis, con iconos de Azure)
2. **Microsoft Visio** (profesional)
3. **Lucidchart** (cloud)
4. **Contratar un diseñador** especializado

---

## 🎨 Especificación 1: Diagrama de Arquitectura del Sistema

### Descripción:
Diagrama completo de la arquitectura del sistema de invitaciones, mostrando todos los componentes de Azure y sus interacciones.

### Componentes (con iconos oficiales de Azure):

#### Frontend (Capa de Presentación):
```
┌─────────────────────────────────────────┐
│  Azure Static Web Apps                  │
│  • React 19 + TypeScript                │
│  • Vite build                           │
│  • URL: portal.company.com             │
└─────────────────────────────────────────┘
```
**Icono**: Azure Static Web Apps (azul)

#### Backend (Capa de Aplicación):
```
┌─────────────────────────────────────────┐
│  Azure App Service                      │
│  • ASP.NET Core 8 API                  │
│  • InvitationController                │
│  • InvitationService                   │
└─────────────────────────────────────────┘
```
**Icono**: Azure App Service (azul)

#### Capa de Datos:
```
┌──────────────────────┐  ┌──────────────────────┐
│  Azure SQL Database  │  │  Azure Cosmos DB     │
│  • VendorInvitations │  │  • InvitationArtifacts│
│  • VendorApplications│  │  • DomainEvents      │
└──────────────────────┘  └──────────────────────┘
```
**Iconos**: 
- Azure SQL Database (azul)
- Azure Cosmos DB (morado/azul)

#### Capa de Integración:
```
┌──────────────────────┐  ┌──────────────────────┐  ┌────────────────────────┐
│  Azure Service Bus   │  │  Azure Functions     │  │  Azure Communication   │
│  • invitation-emails │  │  • SendInvitationEmail│ │  Services              │
│  • vendor-changes    │  │  • Background Tasks  │  │  • Email Delivery      │
└──────────────────────┘  └──────────────────────┘  └────────────────────────┘
```
**Iconos**:
- Azure Service Bus (verde/azul)
- Azure Functions (amarillo/azul)
- Azure Communication Services (azul)

#### Seguridad:
```
┌─────────────────────────────────────────┐
│  Azure Key Vault                        │
│  • Email Credentials                   │
│  • Connection Strings                  │
│  • API Keys                            │
└─────────────────────────────────────────┘
```
**Icono**: Azure Key Vault (naranja/azul)

### Conexiones:

1. **Static Web Apps → App Service**
   - Tipo: HTTP/REST API
   - Color: Azul
   - Etiqueta: "API Calls"

2. **App Service → SQL Database**
   - Tipo: Entity Framework Core
   - Color: Azul
   - Etiqueta: "Store Metadata"

3. **App Service → Cosmos DB**
   - Tipo: Cosmos SDK
   - Color: Morado
   - Etiqueta: "Store Artifacts & Events"

4. **App Service → Service Bus**
   - Tipo: Async Message
   - Color: Verde
   - Etiqueta: "Queue Email Message"

5. **Service Bus → Functions**
   - Tipo: Service Bus Trigger
   - Color: Verde
   - Etiqueta: "Process Message"

6. **Functions → Communication Services**
   - Tipo: SDK Call
   - Color: Azul
   - Etiqueta: "Send Email"

7. **App Service → Key Vault**
   - Tipo: Managed Identity
   - Color: Naranja
   - Etiqueta: "Read Secrets"

8. **Functions → Key Vault**
   - Tipo: Managed Identity
   - Color: Naranja
   - Etiqueta: "Read Secrets"

### Layout Sugerido:
```
┌─────────────────────────────────────────────────────────────────┐
│                    SISTEMA DE INVITACIONES                       │
│                    Arquitectura en Azure                        │
└─────────────────────────────────────────────────────────────────┘

        ┌─────────────────────────┐
        │  Static Web Apps        │ Frontend Layer
        │  (React)                │
        └───────────┬─────────────┘
                    │ HTTP/REST
                    ↓
        ┌─────────────────────────┐
        │  App Service            │ Application Layer
        │  (ASP.NET Core API)     │
        └─┬───────┬───────┬───────┘
          │       │       │
    ┌─────┴─┐  ┌──┴───┐  ┌┴──────┐
    │ SQL   │  │Cosmos│  │Service│ Data & Integration
    │Database│  │  DB  │  │  Bus  │
    └───────┘  └──────┘  └───┬───┘
                              │
                    ┌─────────┴─────┐
                    │  Functions    │ Serverless
                    └───────┬───────┘
                            │
                    ┌───────┴─────────┐
                    │ Communication   │ Services
                    │    Services     │
                    └─────────────────┘

        ┌─────────────────────────┐
        │   Key Vault             │ Security
        │   (Secrets)             │
        └─────────────────────────┘
```

### Estilo:
- **Fondo**: Blanco o gris muy claro (#F3F2F1)
- **Componentes**: Cajas rectangulares con bordes redondeados
- **Colores**: Paleta oficial de Azure (#0078D4 principal)
- **Fuente**: Segoe UI, 12-14pt
- **Iconos**: SVG oficiales de Azure (descargar de Microsoft)
- **Sombras**: Sombra suave para dar profundidad
- **Grid**: Alineación perfecta en grid

### Tamaño de Exportación:
- **Ancho**: 1920px mínimo
- **Alto**: 1080px mínimo
- **Formato**: PNG (300 DPI) o SVG
- **Orientación**: Horizontal (landscape)

---

## 🎨 Especificación 2: Diagrama de Flujo de Secuencia

### Descripción:
Diagrama de secuencia mostrando el flujo completo desde que un admin crea una invitación hasta que el vendor completa el registro.

### Actores/Componentes:

1. **Admin/Approver** (Persona - icono usuario)
2. **Portal Frontend** (Azure Static Web Apps)
3. **Backend API** (Azure App Service)
4. **SQL Database** (Azure SQL)
5. **Cosmos DB** (Azure Cosmos DB)
6. **Service Bus** (Azure Service Bus)
7. **Functions** (Azure Functions)
8. **Email Service** (Azure Communication Services)
9. **Vendor** (Persona - icono usuario)

### Secuencia de Pasos:

```
[Admin] → [Portal]: Navigate to "Invite Vendor"
[Admin] → [Portal]: Fill form & submit
[Portal] → [API]: POST /api/invitation/create

[API] → [SQL]: Create VendorInvitation record
[SQL] → [API]: Invitation created ✓

[API] → [Cosmos]: Store InvitationArtifact
[Cosmos] → [API]: Stored ✓

[API] → [Cosmos]: Emit InvitationCreated event
[Cosmos] → [API]: Event stored ✓

[API] → [Service Bus]: Publish email message
[Service Bus] → [API]: Message queued ✓

[API] → [Portal]: Return invitation link
[Portal] → [Admin]: Display link (copy to clipboard)

[Service Bus] → [Functions]: Trigger SendInvitationEmail
[Functions] → [Email Service]: Send invitation email
[Email Service] → [Vendor]: Email delivered 📧

[Vendor] → [Portal]: Click invitation link
[Portal] → [API]: GET /api/invitation/validate/{token}

[API] → [SQL]: Validate token & expiration
[SQL] → [API]: Valid ✓

[API] → [Portal]: Return invitation details
[Portal] → [Vendor]: Show registration form (pre-filled)

[Vendor] → [Portal]: Complete form & submit
[Portal] → [API]: POST /api/invitation/complete/{token}

[API] → [SQL]: Create VendorApplication
[SQL] → [API]: Application created ✓

[API] → [SQL]: Update invitation status = "Completed"
[SQL] → [API]: Updated ✓

[API] → [Cosmos]: Store ApplicationArtifact
[API] → [Cosmos]: Emit InvitationCompleted event

[API] → [Portal]: Success confirmation
[Portal] → [Vendor]: Registration complete ✓
```

### Estilo:
- **Formato**: Diagrama de secuencia UML
- **Líneas de vida**: Verticales con cajas de activación
- **Mensajes**: Flechas horizontales etiquetadas
- **Colores**: Azul para llamadas exitosas, verde para confirmaciones
- **Iconos**: Incluir iconos de Azure pequeños en cada componente

---

## 🎨 Especificación 3: Diagrama de Estados

### Descripción:
Diagrama de máquina de estados mostrando todos los estados posibles de una invitación y las transiciones.

### Estados:

```
[●] → [Pending]

[Pending] → [Accepted]: Vendor validates token
[Pending] → [Expired]: Time exceeded
[Pending] → [Cancelled]: Admin cancels

[Accepted] → [Completed]: Vendor submits application
[Accepted] → [Expired]: Time exceeded

[Expired] → [Pending]: Admin resends

[Completed] → [●]
[Cancelled] → [●]
```

### Detalles de Estados:

1. **Pending** (Amarillo)
   - Invitación creada
   - Token generado
   - Esperando acción del vendor

2. **Accepted** (Azul)
   - Token validado
   - Vendor viendo formulario
   - En proceso de completar

3. **Completed** (Verde)
   - Aplicación enviada
   - Invitation linked a VendorApplication
   - Token ya no válido

4. **Expired** (Rojo)
   - Tiempo de expiración alcanzado
   - Token ya no válido
   - Puede ser reenviado

5. **Cancelled** (Gris)
   - Admin canceló manualmente
   - No se puede usar

### Estilo:
- **Estados**: Rectángulos redondeados
- **Transiciones**: Flechas con etiquetas
- **Colores**: Por estado (verde éxito, rojo error, azul activo, amarillo pendiente)
- **Estado inicial**: Círculo negro relleno
- **Estados finales**: Círculo con borde doble

---

## 📋 Especificación para Diseñador

Si vas a contratar un diseñador, proporciónale:

### Brief del Proyecto:
```
Proyecto: Vendor MDM Portal
Necesidad: 3-7 diagramas de arquitectura profesionales
Tecnología: Microsoft Azure
Estilo: Moderno, corporativo, oficial de Microsoft
Iconos: Oficiales de Azure (SVG)
Colores: Paleta de Azure (#0078D4)
Entregables: PNG (300 DPI) + SVG + archivos fuente
```

### Documentos a Compartir:
1. Este archivo (DIAGRAM_SPECIFICATIONS.md)
2. `docs/features/invitations.md` (con diagramas Mermaid de referencia)
3. Iconos de Azure descargados

### Referencias Visuales:
- Azure Architecture Center: https://learn.microsoft.com/azure/architecture/
- Ejemplos de diagramas profesionales de Microsoft

---

## ✅ Siguiente Paso Recomendado

1. **Opción A - Hazlo tú mismo**:
   - Descarga Draw.io
   - Sigue la guía: `CREATE_PROFESSIONAL_AZURE_DIAGRAMS.md`
   - Usa estas especificaciones como referencia
   - Tiempo: 2-4 horas

2. **Opción B - Contratar diseñador**:
   - Upwork/Fiverr: Busca "Azure architecture diagram designer"
   - Comparte este documento
   - Costo: $100-300 por 5-7 diagramas
   - Tiempo: 3-5 días

3. **Opción C - Usar templates**:
   - Azure Architecture Templates (Visio)
   - Modificar templates existentes
   - Tiempo: 1-2 horas

---

*Estas especificaciones están listas para ser implementadas en cualquier herramienta profesional de diagramas.* 📐

