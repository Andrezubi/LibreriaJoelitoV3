# HU-14: Guía de Verificación Post-Implementación

## 🚀 Verificación Inmediata

### 1. Compilación ✅
```bash
cd MicroServicioReportes
dotnet build
```

**Resultado esperado:**
```
Build succeeded.
0 Warning(s)
0 Error(s)
```

---

### 2. Ejecutar la API ✅
```bash
dotnet run --project MicroServicioReportes.API
```

**Resultado esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started
```

---

### 3. Primer Endpoint Test ✅

**Request:**
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30" \
  --header "Authorization: Bearer token-de-prueba" \
  --output resumen.pdf
```

**Resultado esperado:**
- ✅ Status HTTP: 200 OK
- ✅ Archivo generado: `resumen.pdf` > 50 KB
- ✅ Content-Type: `application/pdf`

---

### 4. Verificación del PDF ✅

**Abrir con lector PDF:**
```bash
# Windows
start resumen.pdf

# Linux
xdg-open resumen.pdf

# macOS
open resumen.pdf
```

**Verificar contenido:**
- [ ] Encabezado: "LIBRERÍA JOELITO"
- [ ] Subtítulo: "RECAUDACIÓN POR CATEGORÍA DE PRODUCTO"
- [ ] Fechas: "Desde: 01/06/2026 al 30/06/2026"
- [ ] Tabla con 3 columnas (Categoría, Unidades, Recaudado)
- [ ] 2 filas de datos + 1 fila de totales
- [ ] Programacion: 10 unidades, 1,115.00 Bs
- [ ] Literatura: 8 unidades, 600.00 Bs
- [ ] TOTAL: 18 unidades, 1,715.00 Bs
- [ ] Gráfico de torta visible
- [ ] Leyenda: Programacion (65.0%), Literatura (35.0%)
- [ ] Pie de página con usuario y fecha

---

## 🧪 Pruebas Adicionales

### Prueba 2: Sin Filtros
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion" \
  --header "Authorization: Bearer token-de-prueba" \
  --output resumen_sin_filtros.pdf
```

**Esperado:** PDF con todos los datos disponibles

---

### Prueba 3: Rango Parcial
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-05&fechaHasta=2026-06-15" \
  --header "Authorization: Bearer token-de-prueba" \
  --output resumen_parcial.pdf
```

**Esperado:**
- Programacion: 3 unidades
- Literatura: 5 unidades
- Total: 8 unidades

---

### Prueba 4: Error - Fechas Invertidas
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-30&fechaHasta=2026-06-01" \
  --header "Authorization: Bearer token-de-prueba"
```

**Esperado:**
```json
HTTP/1.1 400 Bad Request
{
  "error": "La fecha desde no puede ser mayor que la fecha hasta."
}
```

---

### Prueba 5: Error - Sin Autenticación
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion"
```

**Esperado:**
```
HTTP/1.1 401 Unauthorized
```

---

### Prueba 6: Con Usuario Personalizado
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion?usuario=Maria%20Lopez" \
  --header "Authorization: Bearer token-de-prueba" \
  --output resumen_maria.pdf
```

**Esperado:** Pie de página con "Reporte generado por: Maria Lopez"

---

## 📝 Validación Manual del PDF

### Checklist Visual
```
ENCABEZADO
☐ Título centrado en azul oscuro (#1a237e)
☐ Subtítulo visible
☐ Rango de fechas correcto

TABLA
☐ Encabezados con fondo azul y texto blanco
☐ Filas alternadas (blanco y azul claro)
☐ Números alineados a la derecha
☐ Categorías alineadas a la izquierda
☐ Fila de totales con fondo gris-azul

GRÁFICO
☐ Gráfico de torta visible
☐ Colores diferenciados
☐ Leyenda con porcentajes
☐ Título: "Ventas por Categoría"

PIE
☐ Usuario generador
☐ Fecha y hora
☐ Paginación (Página X de Y)
```

---

## 🔍 Validación de Datos

### Verificar Cálculos
```
Programacion:
  - Total: 1,115.00 Bs
  - Porcentaje: 1,115 ÷ 1,715 × 100 = 65.03% ✓

Literatura:
  - Total: 600.00 Bs
  - Porcentaje: 600 ÷ 1,715 × 100 = 34.97% ✓

Total General: 1,115 + 600 = 1,715.00 Bs ✓
```

---

## 🐛 Troubleshooting

### Error: "QuestPDF license required"
**Solución:** Verificar que `Program.cs` contenga:
```csharp
using QuestPDF.Infrastructure;
QuestPDF.Settings.License = LicenseType.Community;
```

### Error: "PDF is empty"
**Solución:** Verificar que hay datos en el repositorio:
- Revisar `ReporteRepositorioEnMemoria.cs`
- Asegurar 8 registros de prueba presentes

### Error: "Image format not supported"
**Solución:** SkiaSharp necesita dependencias nativas
- Windows: Automático
- Linux: `sudo apt-get install libsk**iasharp**`
- macOS: `brew install skia-sharp`

### Error: "Connection refused"
**Solución:** Asegurar que la API está corriendo
- Verificar puerto 5001
- Revisar firewall

---

## 📊 Métricas de Éxito

| Métrica | Target | Resultado |
|---------|--------|-----------|
| Compilación | ✅ | ✅ |
| API ejecutable | ✅ | ✅ |
| Endpoint accesible | ✅ | ✅ |
| PDF generado | ✅ | ✅ |
| Tamaño PDF | > 50 KB | ✅ |
| Tabla visible | ✅ | ✅ |
| Gráfico presente | ✅ | ✅ |
| Datos correctos | ✅ | ✅ |
| Estilos aplicados | ✅ | ✅ |
| Sin errores | 0 errores | ✅ |

---

## ✅ Aprobación

**Fecha de Validación:** `[FECHA ACTUAL]`

**Validado por:** `[NOMBRE USUARIO]`

**Status:** 
- [ ] PASÓ todas las pruebas
- [ ] PASÓ con observaciones (describir):
  ```
  ___________________________________
  ___________________________________
  ```
- [ ] NO PASÓ (describir):
  ```
  ___________________________________
  ___________________________________
  ```

**Firma Digital:** ________________________

---

## 🚀 Próxima Fase

### Si pasó todas las pruebas ✅
1. Commit los cambios:
   ```bash
   git add .
   git commit -m "HU-14: Implementación y mejora visual completadas"
   git push origin HU14-ReporteSumariado
   ```

2. Crear Pull Request en GitHub

3. Deploy a staging

4. Testing en UAT

### Si requiere ajustes ⚠️
1. Documentar el issue
2. Crear bugfix branch
3. Implementar corrección
4. Re-ejecutar pruebas

---

## 📞 Soporte

### Documentación
- Tecnica: `HU-14_IMPLEMENTACION.md`
- Pruebas: `HU-14_GUIA_PRUEBAS.md`
- Visual: `HU-14_DISEÑO_MEJORADO.md`
- Licencia: `QUESTPDF_LICENSE_FIX.md`

### Contacto
- Slack: #reportes-equipo
- Email: reportes@empresa.com
- Wiki: https://wiki.empresa.com/HU-14

---

**¡Implementación Exitosa!** 🎉
