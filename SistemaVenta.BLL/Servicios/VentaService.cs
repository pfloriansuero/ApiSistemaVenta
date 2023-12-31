﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;
using SistemaVenta.DAL.Repositorios;
using SistemaVenta.DAL.Repositorios.Contrato;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace SistemaVenta.BLL.Servicios
{
    public class VentaService: IVentaService
    {

        private readonly IGenericRepository<DetalleVenta> _detalleVentaRepository;
        private readonly VentaRepository _ventaRepository;
        private readonly IMapper _mapper;

        public VentaService(IGenericRepository<DetalleVenta> detalleVentaRepository, VentaRepository ventaRepository, IMapper mapper)
        {
            _detalleVentaRepository = detalleVentaRepository;
            _ventaRepository = ventaRepository;
            _mapper = mapper;
        }

        public async Task<VentaDTO> Registrar(VentaDTO modelo)
        {
            try
            {
                var ventaGenerada = await _ventaRepository.Registrar(_mapper.Map<Venta>(modelo));
                if (ventaGenerada.IdVenta == 0)
                    throw new TaskCanceledException("No se pudo registrar la venta");

                return _mapper.Map<VentaDTO>(ventaGenerada);
            }
            catch 
            {

                throw;
            }
        }

        public async Task<List<VentaDTO>> Historial(string buscarPor, string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _ventaRepository.Consultar();
            var ListadoResultado = new List<Venta>();

            try
            {
                if (buscarPor == "fecha")
                {
                    DateTime fecha_Inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
                    DateTime fecha_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));

                    ListadoResultado = await query.Where(v => 
                    v.FechaRegistro.Value.Date >= fecha_Inicio.Date &&
                    v.FechaRegistro.Value.Date <= fecha_Fin.Date
                    ).Include(dv => dv.DetalleVenta)
                    .ThenInclude(p=>p.IdProductoNavigation)
                    .ToListAsync();
                }
                else
                {
                    ListadoResultado = await query.Where(v => v.NumeroDocumento == numeroVenta
                    ).Include(dv => dv.DetalleVenta)
                    .ThenInclude(p => p.IdProductoNavigation)
                    .ToListAsync();
                }

            }
            catch
            {

                throw;
            }

            return _mapper.Map<List<VentaDTO>>(ListadoResultado);
        }

        

        public async Task<List<ReporteDTO>> Reporte(string fechaInicio, string fechaFin)
        {
            IQueryable<DetalleVenta> query = await _detalleVentaRepository.Consultar();
            var ListadoResultado = new List<DetalleVenta>();

            try
            {
                DateTime fecha_Inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime fecha_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));

                ListadoResultado = await query
                    .Include(p => p.IdProductoNavigation)
                    .Include(v => v.IdVentaNavigation)
                    .Where(dv =>
                    dv.IdVentaNavigation.FechaRegistro.Value.Date >= fecha_Inicio.Date &&
                    dv.IdVentaNavigation.FechaRegistro.Value.Date <= fecha_Fin.Date).ToListAsync();

            }
            catch
            {

                throw;
            }

            return _mapper.Map<List<ReporteDTO>>(ListadoResultado);
        }
    }
}
