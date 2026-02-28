using Microsoft.AspNetCore.Mvc;

namespace ExpedienteECO.Controllers
{
    public class ComunicacionesController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

