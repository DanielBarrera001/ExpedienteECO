using ExpedienteECO.Entidades;
using ExpedienteECO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpedienteECO.Controllers
{
    public class MedicamentosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LISTADO DE INVENTARIO ---
        public async Task<IActionResult> Index()
        {
            var medicamentos = await _context.Medicamentos
                .OrderBy(m => m.Nombre)
                .ToListAsync();
            return View(medicamentos);
        }

        // --- VISTA DE CREACIÓN Y REINGRESO (GET) ---
        public async Task<IActionResult> Create()
        {
            ViewBag.MedicamentosExistentes = await _context.Medicamentos
                .OrderBy(m => m.Nombre)
                .ToListAsync();
            return View();
        }

        // --- CREAR NUEVO MEDICAMENTO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,StockActual,StockMinimo,Presentacion,Indicaciones")] Medicamento medicamento)
        {
            if (ModelState.IsValid)
            {
                // Buscamos si ya existe por nombre (ignorando mayúsculas/minúsculas)
                var medicamentoExistente = await _context.Medicamentos
                    .FirstOrDefaultAsync(m => m.Nombre.ToLower() == medicamento.Nombre.ToLower());

                if (medicamentoExistente != null)
                {
                    // Si existe, sumamos stock y actualizamos info básica
                    medicamentoExistente.StockActual += medicamento.StockActual;
                    medicamentoExistente.StockMinimo = medicamento.StockMinimo;
                    medicamentoExistente.Presentacion = medicamento.Presentacion;
                    medicamentoExistente.Indicaciones = medicamento.Indicaciones;

                    _context.Update(medicamentoExistente);
                    TempData["Mensaje"] = $"Se han añadido {medicamento.StockActual} unidades a {medicamentoExistente.Nombre}.";
                }
                else
                {
                    // Si es nuevo, lo agregamos
                    _context.Add(medicamento);
                    TempData["Mensaje"] = "Medicamento creado exitosamente.";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo falla, recargamos la lista para la vista
            ViewBag.MedicamentosExistentes = await _context.Medicamentos.OrderBy(m => m.Nombre).ToListAsync();
            return View(medicamento);
        }

        // --- REINGRESO RÁPIDO DESDE LA TABLA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reingreso(int id, int cantidadSumar)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);

            if (medicamento == null) return NotFound();

            if (cantidadSumar > 0)
            {
                medicamento.StockActual += cantidadSumar;
                try
                {
                    _context.Update(medicamento);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = $"Stock actualizado: {medicamento.Nombre} (+{cantidadSumar}).";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicamentoExists(medicamento.Id)) return NotFound();
                    else throw;
                }
            }

            return RedirectToAction(nameof(Create));
        }

        // --- EDITAR MEDICAMENTO (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento == null) return NotFound();

            return View(medicamento);
        }

        // --- EDITAR MEDICAMENTO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,StockActual,StockMinimo,Presentacion,Indicaciones")] Medicamento medicamento)
        {
            if (id != medicamento.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicamento);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Información actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicamentoExists(medicamento.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(medicamento);
        }

        // --- DETALLES EN MODAL (GET) ---
        public async Task<IActionResult> DetailsModal(int? id)
        {
            if (id == null) return NotFound();

            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento == null) return NotFound();

            return PartialView("_DetailsModal", medicamento);
        }

        // --- ELIMINAR MEDICAMENTO (GET) ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medicamento = await _context.Medicamentos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medicamento == null) return NotFound();

            return View(medicamento);
        }

        // --- ELIMINAR MEDICAMENTO (POST) ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento != null)
            {
                _context.Medicamentos.Remove(medicamento);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Medicamento eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MedicamentoExists(int id)
        {
            return _context.Medicamentos.Any(e => e.Id == id);
        }
    }
}