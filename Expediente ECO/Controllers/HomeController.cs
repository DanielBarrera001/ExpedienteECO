using ExpedienteECO.Models;
using ExpedienteECO.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ExpedienteECO.Controllers
{
    public class InsumoMasUsado
    {
        public string NombreInsumo { get; set; }
        public int CantidadUsada { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var hace30Dias = DateTime.Now.AddDays(-30);

            // 1. Insumos con Stock Bajo
            var insumosStockBajo = await _context.Medicamentos
                .Where(i => i.StockActual <= i.StockMinimo)
                .OrderBy(i => i.StockActual)
                .Take(5)
                .ToListAsync();

            // 2. Insumos más utilizados HOY (Optimizado)
            var insumosMasUsadosHoy = await _context.DetalleTratamientos
                .Include(dt => dt.Medicamento)
                .Include(dt => dt.Consulta)
                .Where(dt => dt.Consulta.FechaHora.Date == hoy)
                .GroupBy(dt => dt.Medicamento.Nombre)
                .Select(g => new InsumoMasUsado
                {
                    NombreInsumo = g.Key ?? "Desconocido",
                    CantidadUsada = g.Sum(dt => dt.Cantidad) 
                })
                .OrderByDescending(x => x.CantidadUsada)
                .Take(5)
                .ToListAsync();

            // 3. Métricas Principales
            var totalEstudiantesRegistrados = await _context.Estudiantes.CountAsync();
            var totalConsultasHoy = await _context.Consultas.CountAsync(c => c.FechaHora.Date == hoy);


            var pacientesCriticosHoy = await _context.Estudiantes
                .CountAsync(e => e.TienePadecimiento);

            // 4. Consultas Recientes
            var consultasRecientes = await _context.Consultas
                .Include(c => c.Estudiante)
                .OrderByDescending(c => c.FechaHora)
                .Take(5)
                .ToListAsync();

            // 5. Datos para Gráficos
            var consultasMensuales = await _context.Consultas
                .Where(c => c.FechaHora >= hace30Dias)
                .GroupBy(c => c.FechaHora.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Count() })
                .OrderBy(g => g.Fecha)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                InsumosStockBajo = insumosStockBajo,
                InsumosMasUsadosHoy = insumosMasUsadosHoy,
                TotalEstudiantes = totalEstudiantesRegistrados,
                TotalConsultasHoy = totalConsultasHoy,
                PacientesCriticosHoy = pacientesCriticosHoy,
                ConsultasRecientes = consultasRecientes,

                LabelsConsultas = consultasMensuales.Select(c => c.Fecha.ToString("dd/MM")).ToList(),
                DataConsultas = consultasMensuales.Select(c => c.Total).ToList(),
                LabelsInsumos = insumosStockBajo.Select(i => i.Nombre).ToList(),
                DataInsumos = insumosStockBajo.Select(i => i.StockActual).ToList()
            };

            return View(model);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string mensaje = null)
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                MensajePersonalizado = mensaje
            });
        }

        public async Task<IActionResult> ListarPacientesCriticos()
        {
            var criticos = await _context.Estudiantes
                .Where(e => e.TienePadecimiento)
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();

            return PartialView("_ListaPacientesCriticos", criticos);
        }
    }

    public class DashboardViewModel
    {
        public List<Medicamento> InsumosStockBajo { get; set; } = new();
        public List<InsumoMasUsado> InsumosMasUsadosHoy { get; set; } = new();
        public List<Consulta> ConsultasRecientes { get; set; } = new();

        public int TotalEstudiantes { get; set; }
        public int TotalConsultasHoy { get; set; }
        public int PacientesCriticosHoy { get; set; }

        public List<string> LabelsConsultas { get; set; } = new();
        public List<int> DataConsultas { get; set; } = new();
        public List<string> LabelsInsumos { get; set; } = new();
        public List<int> DataInsumos { get; set; } = new();
    }
}