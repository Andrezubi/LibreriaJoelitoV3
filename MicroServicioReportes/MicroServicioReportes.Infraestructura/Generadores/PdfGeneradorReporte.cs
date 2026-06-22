using MicroServicioReportes.Dominio.Entidades;
using MicroServicioReportes.Dominio.Interfaces;
using System.Globalization;
using System.Text;

namespace MicroServicioReportes.Infraestructura.Generadores;

public class PdfGeneradorReporte : IGeneradorReporte
{
    public string ContentType => "application/pdf";
    public string Extension => ".pdf";

    public byte[] Generar(DocumentoReporte documento)
    {
        var lineas = ConstruirLineas(documento);
        var contenido = ConstruirContenidoPdf(lineas);
        return ConstruirArchivoPdf(contenido);
    }

    private static List<string> ConstruirLineas(DocumentoReporte documento)
    {
        var lineas = new List<string>
        {
            documento.LogoTexto,
            documento.Titulo,
            documento.Subtitulo,
            $"Estado: {documento.EstadoDocumento}",
            $"Fecha generacion: {documento.FechaGeneracion:dd/MM/yyyy HH:mm}",
            $"Usuario: {documento.UsuarioGenerador}",
            string.Empty
        };

        if (documento.DatosGenerales.Any())
        {
            lineas.Add("DATOS GENERALES");
            lineas.AddRange(documento.DatosGenerales.Select(c => $"{c.Etiqueta}: {c.Valor}"));
            lineas.Add(string.Empty);
        }

        foreach (var tabla in documento.Tablas)
        {
            lineas.Add(tabla.Titulo.ToUpperInvariant());
            lineas.Add(string.Join(" | ", tabla.Columnas));

            foreach (var fila in tabla.Filas)
            {
                var valores = tabla.Columnas.Select(columna =>
                    fila.TryGetValue(columna, out var valor) ? valor : string.Empty);
                lineas.Add(string.Join(" | ", valores));
            }

            lineas.Add(string.Empty);
        }

        if (documento.Resumen.Any())
        {
            lineas.Add("RESUMEN");
            lineas.AddRange(documento.Resumen.Select(c => $"{c.Etiqueta}: {c.Valor}"));
            lineas.Add(string.Empty);
        }

        foreach (var grafico in documento.Graficos)
        {
            lineas.Add($"{grafico.Titulo.ToUpperInvariant()} ({grafico.Tipo})");
            foreach (var valor in grafico.Valores)
            {
                lineas.Add($"{valor.Etiqueta}: {valor.Valor}");
            }

            lineas.Add(string.Empty);
        }

        lineas.Add(documento.PiePagina);
        return lineas;
    }

    private static string ConstruirContenidoPdf(IReadOnlyCollection<string> lineas)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 10 Tf");
        builder.AppendLine("50 790 Td");

        var primera = true;
        foreach (var linea in lineas.Take(44))
        {
            if (!primera)
            {
                builder.AppendLine("0 -16 Td");
            }

            builder.AppendLine($"({EscaparPdf(NormalizarAscii(linea))}) Tj");
            primera = false;
        }

        builder.AppendLine("ET");
        return builder.ToString();
    }

    private static byte[] ConstruirArchivoPdf(string contenido)
    {
        var stream = new MemoryStream();
        var offsets = new List<long>();

        void Escribir(string texto)
        {
            var bytes = Encoding.ASCII.GetBytes(texto);
            stream.Write(bytes, 0, bytes.Length);
        }

        Escribir("%PDF-1.4\n");

        offsets.Add(stream.Position);
        Escribir("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets.Add(stream.Position);
        Escribir("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets.Add(stream.Position);
        Escribir("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");

        offsets.Add(stream.Position);
        Escribir("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        var contenidoBytes = Encoding.ASCII.GetBytes(contenido);
        offsets.Add(stream.Position);
        Escribir($"5 0 obj\n<< /Length {contenidoBytes.Length} >>\nstream\n");
        stream.Write(contenidoBytes, 0, contenidoBytes.Length);
        Escribir("\nendstream\nendobj\n");

        var xrefOffset = stream.Position;
        Escribir("xref\n0 6\n");
        Escribir("0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            Escribir($"{offset:0000000000} 00000 n \n");
        }

        Escribir($"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return stream.ToArray();
    }

    private static string EscaparPdf(string texto)
    {
        return texto
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private static string NormalizarAscii(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var caracter in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
            if (categoria == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(caracter <= 127 ? caracter : '?');
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
