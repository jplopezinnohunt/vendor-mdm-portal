# 🔄 Cómo Reiniciar el Backend

## Método 1: Si el backend está corriendo en una terminal

1. **Ve a la terminal donde está corriendo el backend**
2. **Presiona**: `Ctrl + C` (o `Cmd + C` en Mac)
3. **Espera** a que se detenga completamente
4. **Ejecuta de nuevo**:
   ```bash
   cd backend/VendorMdm.Api
   dotnet run
   ```

---

## Método 2: Detener proceso y reiniciar

### Paso 1: Detener el backend

**Opción A - Buscar y matar el proceso:**
```bash
# Encuentra el proceso
lsof -ti:5001

# Detén el proceso (reemplaza PID con el número que salió)
kill -9 PID
```

**Opción B - Detener todos los procesos de dotnet:**
```bash
pkill -f "dotnet.*VendorMdm.Api"
```

### Paso 2: Reiniciar el backend

```bash
cd /Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
dotnet run
```

---

## Método 3: Usar el script (si existe)

```bash
cd /Users/jplopez/projects/vendor-mdm-portal
./start-backend.sh
```

---

## Verificar que está corriendo

Después de reiniciar, deberías ver:
```
Now listening on: http://localhost:5001
Swagger UI: http://localhost:5001/swagger
```

---

## Después de reiniciar

1. ✅ El backend cargará la nueva configuración de email
2. ✅ Los emails se enviarán usando SMTP
3. ✅ Prueba enviando una invitación

---

## Troubleshooting

### "Port already in use"
```bash
# Detén el proceso en el puerto 5001
lsof -ti:5001 | xargs kill -9
```

### "dotnet: command not found"
```bash
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
```

### Backend no inicia
- Verifica que no haya errores en la configuración
- Revisa los logs en la terminal
- Asegúrate de que el puerto 5001 esté libre

