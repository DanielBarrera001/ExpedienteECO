using System.ComponentModel.DataAnnotations;

namespace ExpedienteECO.Entidades
{
    public class Estudiante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El grado es obligatorio")]
        public string Grado { get; set; }

        [Required(ErrorMessage = "La sección es obligatoria")]
        public string Seccion { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Especifique padecimientos o ponga 'Ninguno'")]
        public string PadecimientosCronicos { get; set; }

        public bool TienePadecimiento { get; set; }

        [Required(ErrorMessage = "Especifique alergias o ponga 'Ninguna'")]
        public string Alergias { get; set; }

        [Required(ErrorMessage = "Especifique medicación o ponga 'Ninguna'")]
        public string MedicacionPermanente { get; set; }

        [Required(ErrorMessage = "El teléfono es vital para emergencias")]
        public string TelefonoResponsable { get; set; }

        [Required(ErrorMessage = "El nombre del padre es obligatorio")]
        public string NombrePadre { get; set; }

        public string? FotoPath { get; set; }

        public string? NotasEspeciales { get; set; }

        public virtual ICollection<Consulta>? Consultas { get; set; }
    }
}