using ExpedienteECO.Entidades;
using ExpedienteECO.Models;
using ExpedienteECO.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ... (mismos usings)

namespace ExpedienteECO.Controllers
{
    public class MedicamentosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LISTADO DE INVENTARIO (Acceso para todos) ---
        public async Task<IActionResult> Index()
        {
            var medicamentos = await _context.Medicamentos
                .OrderBy(m => m.Nombre)
                .ToListAsync();
            return View(medicamentos);
        }

        // --- VISTA DE CREACIÓN (Solo Admin) ---
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Create()
        {
            ViewBag.MedicamentosExistentes = await _context.Medicamentos
                .OrderBy(m => m.Nombre)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Create([Bind("Nombre,StockActual,StockMinimo,Presentacion,Indicaciones")] Medicamento medicamento)
        {
            // ... (lógica de creación igual)
            if (ModelState.IsValid)
            {
                var medicamentoExistente = await _context.Medicamentos
                    .FirstOrDefaultAsync(m => m.Nombre.ToLower() == medicamento.Nombre.ToLower());

                if (medicamentoExistente != null)
                {
                    medicamentoExistente.StockActual += medicamento.StockActual;
                    medicamentoExistente.StockMinimo = medicamento.StockMinimo;
                    medicamentoExistente.Presentacion = medicamento.Presentacion;
                    medicamentoExistente.Indicaciones = medicamento.Indicaciones;
                    _context.Update(medicamentoExistente);
                }
                else
                {
                    _context.Add(medicamento);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MedicamentosExistentes = await _context.Medicamentos.OrderBy(m => m.Nombre).ToListAsync();
            return View(medicamento);
        }

        // --- REINGRESO RÁPIDO (Solo Admin) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Reingreso(int id, int cantidadSumar)
        {
            // ... (lógica de reingreso igual)
            return RedirectToAction(nameof(Create));
        }

        // --- EDITAR MEDICAMENTO (Solo Admin) ---
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento == null) return NotFound();
            return View(medicamento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,StockActual,StockMinimo,Presentacion,Indicaciones")] Medicamento medicamento)
        {
            // ... (lógica de edición igual)
            return RedirectToAction(nameof(Index));
        }

        // --- DETALLES EN MODAL (Acceso para todos) ---
        public async Task<IActionResult> DetailsModal(int? id)
        {
            if (id == null) return NotFound();
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento == null) return NotFound();
            return PartialView("_DetailsModal", medicamento);
        }

        // --- ELIMINAR MEDICAMENTO (Solo Admin) ---
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var medicamento = await _context.Medicamentos.FirstOrDefaultAsync(m => m.Id == id);
            if (medicamento == null) return NotFound();
            return View(medicamento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento != null)
            {
                _context.Medicamentos.Remove(medicamento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MedicamentoExists(int id) => _context.Medicamentos.Any(e => e.Id == id);
    }
}