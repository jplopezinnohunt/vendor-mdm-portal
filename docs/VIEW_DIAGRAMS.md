# 👁️ Cómo Ver los Diagramas Visuales

Esta guía explica todas las formas de visualizar los diagramas Mermaid incluidos en la documentación.

---

## 🎯 Formas de Visualizar los Diagramas

### 1. ✅ En VS Code (Recomendado para Desarrollo)

VS Code puede renderizar diagramas Mermaid directamente si tienes las extensiones correctas.

#### Opción A: Markdown Preview Mermaid Support

1. **Instalar extensión**:
   - Abre VS Code
   - Ve a Extensions (Cmd+Shift+X / Ctrl+Shift+X)
   - Busca: **"Markdown Preview Mermaid Support"**
   - O instala: `bierner.markdown-mermaid`

2. **Abrir preview**:
   - Abre el archivo `docs/features/invitations.md`
   - Click derecho → **"Open Preview"** o presiona `Cmd+Shift+V` (Mac) / `Ctrl+Shift+V` (Windows)
   - Los diagramas se renderizarán automáticamente

#### Opción B: Mermaid Preview (Más Avanzado)

1. **Instalar extensión**:
   - Busca: **"Mermaid Preview"** por `vstirbu.vscode-mermaid-preview`
   - O: **"Mermaid Markdown Syntax Highlighting"**

2. **Usar**:
   - Click derecho en el archivo `.md`
   - Selecciona **"Open Preview to the Side"**
   - O usa el comando `Mermaid: Preview`

---

### 2. 🌐 En GitHub (Automático)

**GitHub renderiza diagramas Mermaid automáticamente** cuando subes los archivos.

#### Ver en GitHub:

1. **Sube los archivos** al repositorio:
   ```bash
   git add docs/features/invitations.md
   git commit -m "Add invitations documentation with diagrams"
   git push
   ```

2. **Abre el archivo en GitHub**:
   - Navega a: `https://github.com/tu-usuario/vendor-mdm-portal/blob/main/docs/features/invitations.md`
   - Los diagramas se mostrarán automáticamente renderizados

3. **Ventajas**:
   - ✅ Renderizado automático
   - ✅ Sin necesidad de extensiones
   - ✅ Funciona en cualquier navegador
   - ✅ Compatible con todos los tipos de diagramas Mermaid

---

### 3. 🔧 Herramientas Online (Sin Instalar Nada)

#### Opción A: Mermaid Live Editor

1. **Abrir editor online**:
   - Ve a: https://mermaid.live/
   
2. **Copiar y pegar código**:
   - Abre `docs/features/invitations.md`
   - Copia el código del diagrama (lo que está entre ` ```mermaid ` y ` ``` `)
   - Pégalo en el editor
   - El diagrama se renderiza instantáneamente

3. **Exportar** (opcional):
   - Puedes exportar como PNG, SVG o compartir link

#### Opción B: GitHub Gist

1. **Crear un Gist**:
   - Ve a: https://gist.github.com/
   - Crea un nuevo gist con extensión `.md`
   - GitHub renderizará los diagramas automáticamente

---

### 4. 📖 En Cursor/Editores con Preview de Markdown

Si estás usando Cursor o un editor con soporte Markdown:

1. **Abrir preview**:
   - Busca el comando de preview de Markdown
   - Generalmente: `Cmd+Shift+V` (Mac) o `Ctrl+Shift+V` (Windows)

2. **Verificar soporte Mermaid**:
   - Algunos editores pueden necesitar extensiones adicionales
   - Consulta la documentación de tu editor específico

---

### 5. 🔍 Ver Diagramas Específicos

Puedes ver un diagrama específico copiando solo ese bloque:

#### Ejemplo: Ver el Diagrama de Arquitectura

1. Abre `docs/features/invitations.md`
2. Busca la sección "Diagrama de Arquitectura"
3. Copia solo el código entre:
   ` ```mermaid `
   y
   ` ``` `
4. Pégalo en https://mermaid.live/

---

## 📋 Lista de Diagramas Incluidos

En `docs/features/invitations.md` encontrarás:

1. **Diagrama de Arquitectura del Sistema** (línea ~36)
2. **Flujo End-to-End (Diagrama de Secuencia)** (línea ~90)
3. **Flujo de Usuario - Admin/Approver** (línea ~150)
4. **Flujo de Usuario - Vendor** (línea ~190)
5. **Diagrama de Estados** (línea ~230)
6. **Flujo de Seguridad** (línea ~280)
7. **Integración con Arquitectura Híbrida** (línea ~320)

---

## 🚀 Forma Más Rápida (Recomendada)

### Para Desarrollo Local:

1. **Instala la extensión en VS Code**:
   ```
   Name: Markdown Preview Mermaid Support
   ID: bierner.markdown-mermaid
   ```

2. **Abre el archivo**:
   ```bash
   code docs/features/invitations.md
   ```

3. **Abre preview**:
   - `Cmd+Shift+V` (Mac) o `Ctrl+Shift+V` (Windows)
   - ¡Listo! Verás todos los diagramas renderizados

### Para Compartir/Publicar:

1. **Sube a GitHub** - Los diagramas se renderizan automáticamente
2. O usa **Mermaid Live Editor** para verlos online

---

## 🧪 Probar que Funciona

### Test Rápido:

1. Crea un archivo de prueba `test-diagram.md`:
   ```markdown
   # Test Diagram
   
   ```mermaid
   graph LR
       A[Start] --> B[End]
   ```
   ```

2. Si puedes ver el diagrama en preview, ¡funciona! ✅

---

## 🔧 Troubleshooting

### Los diagramas no se muestran en VS Code

**Solución**:
1. Instala la extensión "Markdown Preview Mermaid Support"
2. Reinicia VS Code
3. Vuelve a abrir el preview

### Los diagramas no se muestran en GitHub

**Causa común**: GitHub soporta Mermaid, pero verifica:
1. El código está entre ` ```mermaid ` y ` ``` ` (sin espacios extra)
2. El archivo tiene extensión `.md` o `.markdown`
3. El repositorio no tiene restricciones que bloqueen el renderizado

### Quiero exportar los diagramas como imágenes

**Solución**:
1. Usa https://mermaid.live/
2. Copia el código del diagrama
3. Pega en el editor
4. Click en "Actions" → "Download PNG/SVG"

---

## 📚 Recursos Adicionales

- **Mermaid Documentation**: https://mermaid.js.org/
- **Mermaid Live Editor**: https://mermaid.live/
- **GitHub Mermaid Support**: https://github.blog/2022-02-14-include-diagrams-markdown-files-mermaid/
- **VS Code Extensions**: Busca "mermaid" en VS Code Marketplace

---

## ✅ Resumen

| Método | Facilidad | Mejor Para |
|--------|-----------|------------|
| **GitHub** | ⭐⭐⭐⭐⭐ | Compartir y documentación pública |
| **VS Code + Extensión** | ⭐⭐⭐⭐ | Desarrollo local |
| **Mermaid Live Editor** | ⭐⭐⭐⭐⭐ | Ver sin instalar nada |
| **Otros editores** | ⭐⭐⭐ | Depende del editor |

**Recomendación**: 
- Para ver ahora mismo: https://mermaid.live/ (pega el código)
- Para desarrollo: Instala extensión en VS Code
- Para compartir: Sube a GitHub

---

*¿Necesitas ayuda? Consulta la documentación de Mermaid o prueba con el editor online.*

