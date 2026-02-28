namespace ExpedienteECO.Models
{
    public class EstudianteViewModel
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; }

        public string Grado { get; set; } = string.Empty;

        public string Seccion { get; set; } = "A";

        public string PadecimientosCronicos { get; set; }

        public string TelefonoResponsable { get; set; }


        public int TotalVisitas { get; set; }
        public string? FotoPath { get; internal set; }
        public bool TienePadecimiento { get; set; }
        public string PadecimientoCronico { get; set; }
    }
}