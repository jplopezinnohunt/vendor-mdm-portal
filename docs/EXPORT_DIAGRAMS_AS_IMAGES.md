# 📸 Exportar Diagramas como Imágenes - Guía Rápida

## ⚡ Método Más Rápido (2 minutos)

### Paso 1: Abre Mermaid Live Editor
👉 https://mermaid.live/

### Paso 2: Copia el Diagrama
1. Abre: `docs/features/invitations.md`
2. Busca cualquier diagrama (busca ````mermaid`)
3. Copia TODO el código entre ` ```mermaid ` y ` ``` `

### Paso 3: Pega y Exporta
1. Pega en mermaid.live (se renderiza automáticamente)
2. Click en **"Actions"** → **"Download PNG"** o **"Download SVG"**
3. ¡Listo! Tienes una imagen profesional

---

## 🎯 Diagramas Disponibles para Exportar

En `docs/features/invitations.md` hay **7 diagramas**:

1. **Diagrama de Arquitectura** (línea ~36)
2. **Flujo End-to-End** (línea ~90)
3. **Flujo Admin/Approver** (línea ~150)
4. **Flujo Vendor** (línea ~190)
5. **Diagrama de Estados** (línea ~230)
6. **Flujo de Seguridad** (línea ~280)
7. **Integración Arquitectura Híbrida** (línea ~320)

---

## 💡 Consejos para Mejor Calidad

1. **Ajusta el zoom** antes de exportar (zoom out para ver todo)
2. **Usa SVG** para mejor calidad (escalable)
3. **Usa PNG** si necesitas opacidad o fondos
4. **Exporta en alta resolución** (1920px+ de ancho)

---

## 📁 Dónde Guardar las Imágenes

Crea esta estructura:
```
docs/
└── images/
    └── diagrams/
        ├── invitation-architecture.png
        ├── invitation-flow-sequence.png
        ├── invitation-admin-flow.png
        └── ...
```

---

## 🔗 Más Información

Para método avanzado con scripts, consulta: [`GENERATE_DIAGRAM_IMAGES.md`](./GENERATE_DIAGRAM_IMAGES.md)

---

*¡Genera imágenes profesionales en minutos!* 🎨

