using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpedienteECO.Entidades
{
    public class DetalleTratamiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConsultaId { get; set; }

        [ForeignKey("ConsultaId")]
        public virtual Consulta Consulta { get; set; }

        [Required]
        public int MedicamentoId { get; set; }

        [ForeignKey("MedicamentoId")]
        public virtual Medicamento Medicamento { get; set; }

        [Required]
        public int Cantidad { get; set; }

    }
}