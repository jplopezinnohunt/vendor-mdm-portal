# 🏗️ Architecture

Documentación sobre principios arquitectónicos y estructura del proyecto.

## 📋 Índice

1. [Principios Arquitectónicos](./principles.md) - Patrones obligatorios y arquitectura híbrida
2. [Estructura del Proyecto](./project-structure.md) - Organización del código y evaluación

---

## 🎯 Principios Clave

### Arquitectura Híbrida

El proyecto utiliza una **Arquitectura Híbrida** combinando:

- **Azure SQL** - Para datos estructurados y relaciones
- **Azure Cosmos DB** - Para documentos flexibles y eventos
- **Azure Service Bus** - Para procesamiento asíncrono

👉 Consulta: [Principios Arquitectónicos](./principles.md) para detalles completos

### Patrones Obligatorios

Todos los features deben seguir el patrón:

```
SQL Database → Cosmos Artifacts → Cosmos Events → Service Bus
```

Este patrón asegura:
- ✅ Auditoría completa
- ✅ Event sourcing
- ✅ Flexibilidad de esquema
- ✅ Integración desacoplada

---

## 📚 Documentos Principales

### [Principios Arquitectónicos](./principles.md)

Documentación detallada sobre:
- Arquitectura híbrida (SQL + Cosmos DB)
- Patrones obligatorios para todos los features
- Templates de código
- Ejemplos prácticos

### [Estructura del Proyecto](./project-structure.md)

Evaluación completa de:
- Organización del código
- Estructura de carpetas
- Mejores prácticas
- Recomendaciones

---

## 🔍 Buscar Información

| Tema | Documento |
|------|-----------|
| Arquitectura híbrida | [Principios](./principles.md) |
| Patrón SQL → Cosmos | [Principios](./principles.md) |
| Estructura de carpetas | [Estructura](./project-structure.md) |
| Evaluación del proyecto | [Estructura](./project-structure.md) |

---

## 🆘 Preguntas Comunes

### ¿Por qué arquitectura híbrida?
→ Consulta [Principios Arquitectónicos → Por qué es obligatorio](./principles.md#why-this-is-mandatory)

### ¿Cómo implemento un nuevo feature?
→ Sigue el patrón en [Principios Arquitectónicos → Code Template](./principles.md#code-template-mandatory)

### ¿Dónde debo poner mi código?
→ Revisa [Estructura del Proyecto](./project-structure.md)

---

## 🔗 Enlaces Relacionados

- [Azure Infrastructure](../azure/infrastructure.md) - Componentes Azure
- [Getting Started](../getting-started/README.md) - Setup inicial
- [Features](../features/README.md) - Características implementadas

---

*¿Dudas sobre arquitectura? Lee [Principios Arquitectónicos](./principles.md) para detalles completos.*

