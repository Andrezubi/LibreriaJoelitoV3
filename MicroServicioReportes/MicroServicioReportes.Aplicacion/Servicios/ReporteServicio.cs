using MicroServicioReportes.Aplicacion.Factoria;
using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;
using MicroServicioReportes.Dominio.Entidades.DTOs;
using MicroServicioReportes.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioReportes.Aplicacion.Servicios
{
    public class ReporteServicio : IReporteServicio
    {
        private readonly IReporteServicio _repositorio;

        private readonly IReporteBuilder _listaUsuariosBuilder;
        private readonly IReporteBuilder _resumenUsuariosBuilder;

        public ReporteServicio(IReporteServicio repositorio, IReporteBuilder listaUsuariosBuilder, IReporteBuilder resumenUsuariosBuilder)
        {
            _repositorio = repositorio;
            _listaUsuariosBuilder = listaUsuariosBuilder;
            _resumenUsuariosBuilder = resumenUsuariosBuilder;
        }
        public async Task<ReporteResponseDto> GenerarListaUsuariosAsync(
           ReporteRequestDto request)
        {
            var usuarios = await _repositorio.ObtenerUsuariosAsync();
            var config = MapConfig(request, "Lista de Usuarios");

            // El Director dirige pasos sobre la interfaz, sin saber el concreto
            _listaUsuariosBuilder
                .AgregarEncabezado(config)
                .AgregarDetalle()
                .AgregarResumen()
                .AgregarGrafico()
                .AgregarPie(request.Usuario);

            return Renderizar(
                _listaUsuariosBuilder.Construir(), request, "ListaUsuarios");
        }

        public async Task<ReporteResponseDto> GenerarResumenUsuariosAsync(
            ReporteRequestDto request)
        {
            var usuarios = await _repositorio.ObtenerUsuariosAsync();
            var config = MapConfig(request, "Resumen de Usuarios");

            _resumenUsuariosBuilder
                .AgregarEncabezado(config)
                .AgregarDetalle()
                .AgregarResumen()
                .AgregarGrafico()
                .AgregarPie(request.Usuario);

            return Renderizar(
                _resumenUsuariosBuilder.Construir(), request, "ResumenUsuarios");
        }

        // Stubs ventas
        public Task<ReporteResponseDto> GenerarComprobanteVentaAsync(ReporteRequestDto r)
            => throw new NotImplementedException("Pendiente: Servicio_Ventas.");
        public Task<ReporteResponseDto> GenerarListaVentasAsync(ReporteRequestDto r)
            => throw new NotImplementedException("Pendiente: Servicio_Ventas.");
        public Task<ReporteResponseDto> GenerarResumenVentasAsync(ReporteRequestDto r)
            => throw new NotImplementedException("Pendiente: Servicio_Ventas.");

        // Helpers
        private static ConfigReporteDto MapConfig(
            ReporteRequestDto r, string titulo) => new()
            {
                FechaDesde = r.FechaDesde,
                FechaHasta = r.FechaHasta,
                Usuario = r.Usuario,
                TipoReporte = titulo
            };

        private static ReporteResponseDto Renderizar(
            DocumentoReporte doc,
            ReporteRequestDto request,
            string nombreBase)
        {
            GeneradorCreador creador = request.Format.ToLower() switch
            {
                "excel" => new ExcelCreador(),
                _ => new PdfGenerador()
            };

            return new ReporteResponseDto
            {
                Archivo = creador.Generar(doc),
                ContentType = creador.ObtenerContentType(),
                NombreArchivo = $"{nombreBase}_{DateTime.Now:yyyyMMdd}{creador.ObtenerExtension()}"
            };
        }
    }
}
