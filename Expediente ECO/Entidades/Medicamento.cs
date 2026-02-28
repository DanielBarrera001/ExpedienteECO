using System.ComponentModel.DataAnnotations;

namespace ExpedienteECO.Entidades
{
    public class Medicamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } 

        public int StockActual { get; set; }

        public int StockMinimo { get; set; } 

        public string? Indicaciones { get; set; } 
        public string? Presentacion { get; set; } 
    }
}