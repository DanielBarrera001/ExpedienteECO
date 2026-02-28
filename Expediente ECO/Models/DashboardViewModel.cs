using ExpedienteECO.Entidades;

namespace ExpedienteECO.Models
{
    public class DashboardViewModel
    {
        public int TotalConsultasHoy { get; set; }
        public int PacientesCriticosHoy { get; set; }
        public int TotalEstudiantes { get; set; }
        public List<Medicamento> InsumosStockBajo { get; set; }
        public List<Consulta> ConsultasRecientes { get; set; }
    }
}
