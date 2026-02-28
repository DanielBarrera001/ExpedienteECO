using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpedienteECO.Entidades
{
    public class Consulta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante Estudiante { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El motivo es necesario")]
        public string Motivo { get; set; }

        public string Sintomas { get; set; }
        public string SignosVitales { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string UsuarioId { get; set; }

        // --- NUEVOS CAMPOS PARA EL CONTROLADOR Y REPORTES ---

        // El ID del medicamento (opcional si no se dio nada)
        public int? MedicamentoId { get; set; }

        [ForeignKey("MedicamentoId")]
        public virtual Medicamento? Medicamento { get; set; }

        // Cantidad física utilizada
        public int CantidadUsada { get; set; }

        public virtual ICollection<DetalleTratamiento> InsumosUtilizados { get; set; } = new List<DetalleTratamiento>();
    }
}