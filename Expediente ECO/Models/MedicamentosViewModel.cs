namespace ExpedienteECO.Models
{
    public class MedicamentoViewModel
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public string EstadoStock => StockActual <= 0 ? "Agotado" :
                                     StockActual <= StockMinimo ? "Bajo Stock" : "OK";

        public string ClaseColor => StockActual <= 0 ? "text-danger" :
                                    StockActual <= StockMinimo ? "text-warning" : "text-success";
    }
}