using ExpedienteECO.Entidades;
using System.ComponentModel.DataAnnotations;

namespace ExpedienteECO.Models
{
    public class ConsultaViewModel
    {
        
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        [Required(ErrorMessage = "El motivo de consulta es obligatorio.")]
        public string Motivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe registrar los síntomas observados.")]
        public string Sintomas { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los signos vitales son indispensables.")]
        public string SignosVitales { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar un diagnóstico presuntivo.")]
        public string Diagnostico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique el tratamiento o acción realizada.")]
        public string Tratamiento { get; set; } = string.Empty;

        [Required]
        public string UsuarioId { get; set; } = "Usuario_Clinica";

        // --- Lógica de Insumos (DetalleTratamiento) ---
        public int? MedicamentoId { get; set; } 
        public int CantidadUsada { get; set; } = 0; 

        // Lista para llenar el Select en la vista
        public List<Medicamento>? MedicamentosDisponibles { get; set; }

        // --- Datos de solo lectura para la UI (GET) ---
        public string? NombreEstudiante { get; set; }
        public string? GradoSeccion { get; set; }
        public string? Alergias { get; set; }
        public string? PadecimientosCronicos { get; set; }
        public string? FotoPath { get; set; }
    }
}