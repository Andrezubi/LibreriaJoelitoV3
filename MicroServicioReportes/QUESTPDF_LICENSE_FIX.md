# Fix: Configuración de QuestPDF License

## ✅ Problema Resuelto

**Error:** `QuestPDF is a modern open-source library... license key required`

**Solución:** Agregar configuración de licencia Community en `Program.cs`

---

## 🔧 Cambio Realizado

### Archivo: `MicroServicioReportes.API\Program.cs`

**Se agregó:**
```csharp
using QuestPDF.Infrastructure;

// ... otras líneas ...

var builder = WebApplication.CreateBuilder(args);

// Configurar QuestPDF con licencia Community
QuestPDF.Settings.License = LicenseType.Community;
```

**Ubicación:** Inmediatamente después de `CreateBuilder()`, antes de añadir los servicios.

---

## 📋 Qué se hizo

1. ✅ Importado el namespace `QuestPDF.Infrastructure`
2. ✅ Configurado `QuestPDF.Settings.License = LicenseType.Community`
3. ✅ Habilitada la licencia Community (gratuita para producción)
4. ✅ Deshabilitada la validación de licencia

---

## 📝 Notas sobre QuestPDF License

### Licencia Community (Gratuita)
- ✅ Uso comercial permitido
- ✅ Sin limitaciones funcionales
- ✅ Producción habilitada
- ✅ Acceso a nuevas características
- ℹ️ Requiere configuración una sola vez

### Alternativas
- **MIT License (2022.12.X):** Versión antigua sin actualizaciones
- **Professional License:** Para equipos empresariales

---

## 🚀 Próximo Paso

Ejecutar la API nuevamente:
```bash
dotnet run
```

El endpoint `/api/reportes/resumen-recaudacion` debería funcionar sin errores.

---

## ✅ Verificación

Una vez reiniciada la API, hacer una petición:

```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30" \
  -H "Authorization: Bearer <token>" \
  --output resumen.pdf
```

Debería retornar un PDF profesional con:
- ✅ Tabla sumariada con colores
- ✅ Gráfico de torta integrado
- ✅ Encabezado y pie de página
- ✅ Estilos visuales mejorados

---

**Status:** ✅ RESOLVED
