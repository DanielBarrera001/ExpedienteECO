using ExpedienteECO.Entidades;
using ExpedienteECO.Models;
using ExpedienteECO.Servicios; // Importante para Constantes.RolAdmin
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpedienteECO.Controllers
{
    [Authorize] // Todo el controlador requiere estar logueado
    public class ConsultasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsultasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LISTADO GENERAL DE CONSULTAS ---
        public async Task<IActionResult> Index()
        {
            var consultas = await _context.Consultas
                .Include(c => c.Estudiante)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();

            return View(consultas);
        }

        // --- VISTA DETALLADA (TIPO NOTA MÉDICA) ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var consulta = await _context.Consultas
                .Include(c => c.Estudiante)
                .Include(c => c.InsumosUtilizados)
                    .ThenInclude(d => d.Medicamento)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (consulta == null) return NotFound();

            return View(consulta);
        }

        // --- CREAR CONSULTA (GET) ---
        public async Task<IActionResult> Create(int estudianteId)
        {
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Id == estudianteId);

            if (estudiante == null) return NotFound();

            var model = new ConsultaViewModel
            {
                EstudianteId = estudiante.Id,
                NombreEstudiante = estudiante.NombreCompleto,
                GradoSeccion = $"{estudiante.Grado} - {estudiante.Seccion}",
                Alergias = estudiante.Alergias ?? "Ninguna",
                PadecimientosCronicos = estudiante.PadecimientosCronicos ?? "Ninguno",
                FotoPath = estudiante.FotoPath,
                UsuarioId = User.Identity?.Name ?? "Usuario_Clinica"
            };

            await RellenarMedicamentos(model);
            return View(model);
        }

        // --- CREAR CONSULTA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsultaViewModel model)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var consulta = new Consulta
                    {
                        EstudianteId = model.EstudianteId,
                        FechaHora = DateTime.Now,
                        Motivo = model.Motivo,
                        Sintomas = model.Sintomas,
                        SignosVitales = model.SignosVitales,
                        Diagnostico = model.Diagnostico,
                        Tratamiento = model.Tratamiento,
                        UsuarioId = model.UsuarioId ?? "Usuario_Clinica"
                    };

                    _context.Consultas.Add(consulta);
                    await _context.SaveChangesAsync();

                    if (model.MedicamentoId.HasValue && model.CantidadUsada > 0)
                    {
                        var med = await _context.Medicamentos.FindAsync(model.MedicamentoId);
                        if (med != null)
                        {
                            if (med.StockActual < model.CantidadUsada)
                            {
                                ModelState.AddModelError("", $"Stock insuficiente. Solo quedan {med.StockActual} unidades.");
                                await RellenarDatosEstudiante(model);
                                await RellenarMedicamentos(model);
                                return View(model);
                            }

                            med.StockActual -= model.CantidadUsada;
                            _context.Update(med);

                            var detalle = new DetalleTratamiento
                            {
                                ConsultaId = consulta.Id,
                                MedicamentoId = med.Id,
                                Cantidad = model.CantidadUsada
                            };
                            _context.DetalleTratamientos.Add(detalle);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction("Details", "Estudiantes", new { id = model.EstudianteId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error al guardar la atención: " + ex.Message);
                }
            }

            await RellenarDatosEstudiante(model);
            await RellenarMedicamentos(model);
            return View(model);
        }

        // --- EDITAR CONSULTA (SOLO ADMIN) ---
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var consulta = await _context.Consultas
                .Include(c => c.Estudiante)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (consulta == null) return NotFound();

            var model = new ConsultaViewModel
            {
                Id = consulta.Id,
                EstudianteId = consulta.EstudianteId,
                NombreEstudiante = consulta.Estudiante.NombreCompleto,
                GradoSeccion = $"{consulta.Estudiante.Grado} - {consulta.Estudiante.Seccion}",
                Alergias = consulta.Estudiante.Alergias ?? "Ninguna",
                PadecimientosCronicos = consulta.Estudiante.PadecimientosCronicos ?? "Ninguno",
                FotoPath = consulta.Estudiante.FotoPath,
                Motivo = consulta.Motivo,
                Sintomas = consulta.Sintomas,
                SignosVitales = consulta.SignosVitales,
                Diagnostico = consulta.Diagnostico,
                Tratamiento = consulta.Tratamiento,
                FechaHora = consulta.FechaHora
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Edit(int id, ConsultaViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var consultaDB = await _context.Consultas.FindAsync(id);
                    if (consultaDB == null) return NotFound();

                    consultaDB.Motivo = model.Motivo;
                    consultaDB.Sintomas = model.Sintomas;
                    consultaDB.SignosVitales = model.SignosVitales;
                    consultaDB.Diagnostico = model.Diagnostico;
                    consultaDB.Tratamiento = model.Tratamiento;

                    _context.Update(consultaDB);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }
            await RellenarDatosEstudiante(model);
            return View(model);
        }

        // --- ELIMINAR CONSULTA (SOLO ADMIN) ---
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var consulta = await _context.Consultas.Include(c => c.Estudiante).FirstOrDefaultAsync(m => m.Id == id);
            if (consulta == null) return NotFound();
            return View(consulta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consulta = await _context.Consultas.FindAsync(id);
            if (consulta != null)
            {
                _context.Consultas.Remove(consulta);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- MÉTODOS AUXILIARES ---
        private async Task RellenarMedicamentos(ConsultaViewModel model)
        {
            model.MedicamentosDisponibles = await _context.Medicamentos
                .Where(m => m.StockActual > 0)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        private async Task RellenarDatosEstudiante(ConsultaViewModel model)
        {
            var est = await _context.Estudiantes.FindAsync(model.EstudianteId);
            if (est != null)
            {
                model.NombreEstudiante = est.NombreCompleto;
                model.GradoSeccion = $"{est.Grado} - {est.Seccion}";
                model.Alergias = est.Alergias ?? "Ninguna";
                model.PadecimientosCronicos = est.PadecimientosCronicos ?? "Ninguno";
                model.FotoPath = est.FotoPath;
            }
        }
    }
}