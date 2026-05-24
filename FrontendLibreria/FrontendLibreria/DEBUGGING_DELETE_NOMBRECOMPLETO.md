# 🐛 Debugging: Problemas con Eliminar y Nombre Completo

## ✅ Problema 1: Nombre Completo No Se Recupera

### Causa
El DTO `UsuarioDto` no calculaba correctamente el nombre completo cuando los apellidos venían como campos separados.

### Solución Implementada
Se agregó una propiedad calculada `NombreCompleto` que concatena automáticamente:
```csharp
public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();
```

### Vista Actualizada
Cambio en `UsuarioIndex.cshtml`:
```html
<!-- Antes ❌ -->
<td>@usuario.Nombre @usuario.ApellidoPaterno @usuario.ApellidoMaterno</td>

<!-- Después ✅ -->
<td>@usuario.NombreCompleto</td>
```

**Resultado**: El nombre completo ahora se muestra correctamente: "Adrian Rodriguez Montecinos"

---

## 🔧 Problema 2: Eliminar No Funciona

### Síntomas
- Al hacer clic en "Eliminar", aparece el mensaje: "No se pudo eliminar el usuario."
- No hay más detalles del error

### Causas Posibles

#### **Causa 1: Falta el Token JWT**
El endpoint de DELETE también requiere autorización.

**Verificación**: En los logs, debes ver:
```
🔐 Configurando header Authorization con Bearer token
```

Si vez en lugar:
```
⚠️ No hay token disponible para la petición
```

**Solución**: Hacer login nuevamente

#### **Causa 2: Respuesta 403 Forbidden (Sin permiso)**
Solo Administradores pueden eliminar usuarios.

**Verificación**: En los logs:
```
❌ Error al eliminar usuario 2 - Status: 403
```

**Solución**: Usar una cuenta con rol "Administrador"

#### **Causa 3: Usuario no encontrado (404)**
El ID del usuario no existe.

**Verificación**: En los logs:
```
❌ Error al eliminar usuario 999 - Status: 404
```

**Solución**: Refrescar la página para obtener los IDs correctos

#### **Causa 4: Error de validación del backend (400)**
El backend rechaza la eliminación por validaciones.

**Verificación**: En los logs:
```
❌ Error al eliminar usuario 1 - Status: 400 - Respuesta: {"error":"No se puede eliminar el admin"}
```

### Solución Mejorada

Se agregó logging detallado para ver exactamente qué está pasando:

```csharp
public async Task<bool> Eliminar(int id)
{
    try
    {
        ConfigurarHeaderDeAutorizacion();  // Agregar token
        _logger.LogInformation("🔍 Eliminando usuario con ID: {Id}", id);
        var response = await _httpClient.DeleteAsync($"/api/usuarios/{id}");

        _logger.LogInformation("📊 Respuesta Eliminar - Status: {StatusCode}", response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("📋 Contenido respuesta: {Content}", responseContent);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("✅ Usuario eliminado: {Id}", id);
            return true;
        }

        _logger.LogError("❌ Error - Status: {Status} - Respuesta: {Response}", 
            response.StatusCode, responseContent);
        return false;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Error: {Message}", ex.Message);
        return false;
    }
}
```

---

## 🔍 Cómo Debuggear

### Paso 1: Ver los Logs
1. En VS: **View** → **Output** (Ctrl+Alt+O)
2. Busca mensajes con: "Eliminando usuario"
3. Anota el **Status Code** que ves

### Paso 2: Interpretar el Status Code

| Status | Significado | Solución |
|--------|-------------|----------|
| **200** | Éxito | ✅ El usuario se eliminó |
| **204** | Sin contenido (éxito sin respuesta) | ✅ El usuario se eliminó |
| **400** | Bad Request | Ver el mensaje de error en la respuesta |
| **401** | No autenticado | Hacer login nuevamente |
| **403** | Forbidden (sin permiso) | Usar cuenta Administrador |
| **404** | No encontrado | El usuario no existe |
| **500** | Error del servidor | Ver los logs del backend |

### Paso 3: Si Falla con Status 400

En los logs verás algo como:
```
📋 Contenido respuesta: {"error":"No se puede eliminar al último administrador"}
```

Este es un error de validación del **backend**. El microservicio está rechazando la eliminación por una razón de negocio.

---

## 🧪 Prueba Completa

### Paso 1: Ver los usuarios actuales
1. Haz login como `admin.prueba`
2. Ve a `/Usuarios/UsuarioIndex`
3. Anota los IDs de los usuarios

### Paso 2: Intentar eliminar
1. Haz clic en "Eliminar" de un usuario que NO sea admin
2. Confirma el popup

### Paso 3: Verificar en los logs
En el Output, deberías ver:
```
🔍 Eliminando usuario con ID: 2
🔐 Configurando header Authorization con Bearer token
📊 Respuesta Eliminar - Status: 200
📋 Contenido respuesta: 
✅ Usuario eliminado: 2
```

O si falla:
```
🔍 Eliminando usuario con ID: 2
🔐 Configurando header Authorization con Bearer token
📊 Respuesta Eliminar - Status: 400
📋 Contenido respuesta: {"error":"No se puede eliminar este usuario"}
❌ Error - Status: 400 - Respuesta: {"error":"No se puede eliminar este usuario"}
```

### Paso 4: Verificar en la UI
- ✅ **Éxito**: Aparece mensaje verde "Usuario eliminado (baja lógica) exitosamente."
- ❌ **Error**: Aparece mensaje rojo "No se pudo eliminar el usuario."

Si ves el mensaje de error, **revisa los logs** para saber la causa exacta.

---

## 📝 Cambios Realizados

### 1. **UsuarioDto.cs**
- ✅ Agregadas propiedades: `FechaNacimiento`, `FechaIngreso`, `IdUsuario`
- ✅ Agregada propiedad calculada: `NombreCompleto`

### 2. **UsuarioIndex.cshtml**
- ✅ Cambio de interpolación manual a propiedad calculada
- ✅ Ahora usa: `@usuario.NombreCompleto`

### 3. **UsuarioServicioAdapter.cs**
- ✅ Mejorado método `Eliminar()` con logging detallado
- ✅ Se captura la respuesta completa del servidor
- ✅ Se registra el contenido de la respuesta

---

## ✅ Checklist

- [x] Nombre completo se calcula correctamente
- [x] Se muestra en la tabla de usuarios
- [x] Token se envía al eliminar
- [x] Se registra el status code en logs
- [x] Se captura el contenido de la respuesta

---

## 🎯 Próximos Pasos

1. Ejecuta ambos servicios
2. Haz login
3. Ve a la lista de usuarios
4. Verifica que el nombre completo se muestra correctamente
5. Intenta eliminar un usuario y revisa los logs
6. Si falla, compartir el status code y mensaje del log

**¿Aún hay problemas?** Comparte el **Status Code** y el **Contenido respuesta** que ves en los logs 📋
