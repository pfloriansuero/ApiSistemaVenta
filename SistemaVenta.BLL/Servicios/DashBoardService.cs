using System;
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
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace SistemaVenta.BLL.Servicios
{
    public class DashBoardService: IDashBoardService
    {
        private readonly IGenericRepository<Producto> _productoRepository;
        private readonly VentaRepository _ventaRepository;
        private readonly IMapper _mapper;

        public DashBoardService(IGenericRepository<Producto> productoRepository, VentaRepository ventaRepository, IMapper mapper)
        {
            _productoRepository = productoRepository;
            _ventaRepository = ventaRepository;
            _mapper = mapper;
        }

        private IQueryable<Venta> retornarVentas(IQueryable<Venta> tablaVentas, int restarCantidadDias)
        {
            DateTime? ultimaFecha = tablaVentas.OrderByDescending(v => v.FechaRegistro).Select(v => v.FechaRegistro).First();

            ultimaFecha = ultimaFecha.Value.Date.AddDays(restarCantidadDias);

            return tablaVentas.Where(f => f.FechaRegistro >= ultimaFecha.Value.Date);

        }

        private async Task<int> totalVentasUltimaSemana()
        {
            int total = 0;
            IQueryable<Venta> _ventaQuery = await _ventaRepository.Consultar();

            if(_ventaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);
                total = tablaVenta.Count();
            }
            
            return total;
        }

        private async Task<string> totalIngresosUltimaSemana()
        {
            decimal resultado = 0;
            IQueryable<Venta> _ventaQuery = await _ventaRepository.Consultar();

            if (_ventaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);
                resultado = tablaVenta.Select(t => t.Total).Sum(t => t.Value);
            }

            return Convert.ToString( resultado, new CultureInfo("es-PE"));
            
        }

        private async Task<int> totalProductos()
        {
            IQueryable<Producto> _productoQuery = await _productoRepository.Consultar();
            int total = _productoQuery.Count();

            return total;
            
        }

        private async Task<Dictionary<string,int>> VentasUltimaSemana()
        {
            Dictionary<string,int> resultado = new Dictionary<string,int>();

            IQueryable<Venta> _ventaQuery = await _ventaRepository.Consultar();
            if (_ventaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);

                resultado = tablaVenta
                    .GroupBy(f => f.FechaRegistro.Value.Date)
                    .OrderBy(f => f.Key)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count() })
                    .ToDictionary(keySelector: r => r.fecha, elementSelector: r => r.total);
            }
            return resultado;
        }



        public async Task<DashBoardDTO> Resumen()
        {
            DashBoardDTO vmDashBoard = new DashBoardDTO();

            try
            {

                vmDashBoard.TotalVentas = await totalVentasUltimaSemana();
                vmDashBoard.TotalIngresos = await totalIngresosUltimaSemana();
                vmDashBoard.TotalProductos = await totalProductos();

                List<VentaSemanaDTO> listaVentaSemana = new List<VentaSemanaDTO>();

                foreach (KeyValuePair<string, int> item in await VentasUltimaSemana())
                {
                    listaVentaSemana.Add(new VentaSemanaDTO()
                    {
                        Fecha = item.Key,
                        Total = item.Value
                    });


                }

                vmDashBoard.VentasUltimasSemanas = listaVentaSemana;

            }
            catch 
            {

                throw;
            }

            return vmDashBoard;
        }
    }
}
