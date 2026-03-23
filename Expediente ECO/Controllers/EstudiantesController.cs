using ExpedienteECO.Entidades;
using ExpedienteECO.Models;
using ExpedienteECO.Servicios; // Asegúrate de tener este namespace para Constantes.RolAdmin
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExpedienteECO.Controllers
{
    [Authorize]
    public class EstudiantesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // Lista normalizada de grados
        private readonly List<string> _gradosEscolares = new()
        {
            "Inicial 3", "Parvularia 4", "Parvularia 5", "Parvularia 6",
            "Primero", "Segundo", "Tercero", "Cuarto", "Quinto", "Sexto",
            "Séptimo", "Octavo", "Noveno", "Décimo", "Onceavo"
        };

        public EstudiantesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // --- MÉTODOS DE ACCESO GENERAL (Admin y Doctor) ---

        public async Task<IActionResult> Index(string buscar, string sortOrder)
        {
            ViewBag.NombreSortParm = String.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
            ViewBag.GradoSortParm = sortOrder == "Grado" ? "grado_desc" : "Grado";
            ViewBag.ConsultasSortParm = sortOrder == "Consultas" ? "consultas_desc" : "Consultas";
            ViewBag.Busqueda = buscar;

            var estudiantesQuery = _context.Estudiantes.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                estudiantesQuery = estudiantesQuery.Where(s => s.NombreCompleto.Contains(buscar) || s.Grado.Contains(buscar));
            }

            var listaQuery = estudiantesQuery.Select(s => new EstudianteViewModel
            {
                Id = s.Id,
                NombreCompleto = s.NombreCompleto,
                Grado = s.Grado,
                Seccion = s.Seccion,
                TotalVisitas = s.Consultas.Count(),
                FotoPath = s.FotoPath,
                PadecimientosCronicos = s.PadecimientosCronicos
            });

            if (sortOrder == "Grado" || sortOrder == "grado_desc")
            {
                var listaConPeso = listaQuery.AsEnumerable().OrderBy(s => GetGradoPeso(s.Grado));
                if (sortOrder == "grado_desc")
                    return View(listaConPeso.OrderByDescending(s => GetGradoPeso(s.Grado)).ToList());

                return View(listaConPeso.ToList());
            }

            listaQuery = sortOrder switch
            {
                "nombre_desc" => listaQuery.OrderByDescending(s => s.NombreCompleto),
                "Consultas" => listaQuery.OrderBy(s => s.TotalVisitas),
                "consultas_desc" => listaQuery.OrderByDescending(s => s.TotalVisitas),
                _ => listaQuery.OrderBy(s => s.NombreCompleto),
            };

            return View(await listaQuery.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var estudiante = await _context.Estudiantes.Include(e => e.Consultas).FirstOrDefaultAsync(m => m.Id == id);
            return estudiante == null ? NotFound() : View(estudiante);
        }

        public async Task<IActionResult> BuscarParaConsulta(string term)
        {
            if (string.IsNullOrEmpty(term)) return PartialView("_ResultadosBusqueda", new List<Estudiante>());
            var resultados = await _context.Estudiantes.Where(e => e.NombreCompleto.Contains(term)).Take(5).ToListAsync();
            return PartialView("_ResultadosBusqueda", resultados);
        }

        // --- MÉTODOS SOLO ADMINISTRADOR ---

        [Authorize(Roles = Constantes.RolAdmin)]
        public IActionResult Create()
        {
            ViewBag.Grados = new SelectList(_gradosEscolares);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Create(Estudiante estudiante, IFormFile? fotoFile)
        {
            estudiante.Seccion = "A";
            if (estudiante.TienePadecimiento && (string.IsNullOrEmpty(estudiante.PadecimientosCronicos) || estudiante.PadecimientosCronicos == "Ninguno"))
                estudiante.PadecimientosCronicos = "Pendiente de especificar";
            else if (!estudiante.TienePadecimiento)
                estudiante.PadecimientosCronicos = "Ninguno";

            if (ModelState.IsValid)
            {
                if (fotoFile != null && fotoFile.Length > 0)
                    estudiante.FotoPath = await GuardarArchivo(fotoFile);

                _context.Add(estudiante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Grados = new SelectList(_gradosEscolares);
            return View(estudiante);
        }

        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();
            ViewBag.Grados = new SelectList(_gradosEscolares, estudiante.Grado);
            return View(estudiante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Edit(int id, Estudiante estudiante, IFormFile? fotoFile)
        {
            if (id != estudiante.Id) return NotFound();
            estudiante.Seccion = "A";
            if (!estudiante.TienePadecimiento) estudiante.PadecimientosCronicos = "Ninguno";

            if (ModelState.IsValid)
            {
                try
                {
                    if (fotoFile != null && fotoFile.Length > 0)
                    {
                        // Si ya tiene foto, eliminar la anterior
                        var oldPath = await _context.Estudiantes.Where(e => e.Id == id).Select(e => e.FotoPath).FirstOrDefaultAsync();
                        if (!string.IsNullOrEmpty(oldPath)) EliminarArchivo(oldPath);

                        estudiante.FotoPath = await GuardarArchivo(fotoFile);
                    }
                    _context.Update(estudiante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Estudiantes.Any(e => e.Id == estudiante.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Details), new { id = estudiante.Id });
            }
            ViewBag.Grados = new SelectList(_gradosEscolares, estudiante.Grado);
            return View(estudiante);
        }

        // NUEVO MÉTODO PARA EL MODAL DE FOTO
        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<JsonResult> ActualizarFotoModal(int id, IFormFile fotoFile)
        {
            try
            {
                if (fotoFile == null || fotoFile.Length == 0)
                    return Json(new { success = false, message = "No se seleccionó ninguna imagen." });

                var estudiante = await _context.Estudiantes.FindAsync(id);
                if (estudiante == null)
                    return Json(new { success = false, message = "Estudiante no encontrado." });

                // Eliminar foto antigua si existe
                if (!string.IsNullOrEmpty(estudiante.FotoPath))
                    EliminarArchivo(estudiante.FotoPath);

                // Guardar nueva foto
                estudiante.FotoPath = await GuardarArchivo(fotoFile);
                _context.Update(estudiante);
                await _context.SaveChangesAsync();

                return Json(new { success = true, newPath = estudiante.FotoPath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(m => m.Id == id);
            return estudiante == null ? NotFound() : View(estudiante);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante != null)
            {
                if (!string.IsNullOrEmpty(estudiante.FotoPath)) EliminarArchivo(estudiante.FotoPath);
                _context.Estudiantes.Remove(estudiante);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- HELPERS ---

        private int GetGradoPeso(string grado) => grado switch
        {
            "Inicial 3" => 1,
            "Parvularia 4" => 2,
            "Parvularia 5" => 3,
            "Parvularia 6" => 4,
            "Primero" => 5,
            "Segundo" => 6,
            "Tercero" => 7,
            "Cuarto" => 8,
            "Quinto" => 9,
            "Sexto" => 10,
            "Séptimo" => 11,
            "Octavo" => 12,
            "Noveno" => 13,
            "Décimo" => 14,
            "Onceavo" => 15,
            _ => 99
        };

        private async Task<string> GuardarArchivo(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "fotos");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return "/uploads/fotos/" + fileName;
        }

        private void EliminarArchivo(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, path.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }
    }
}