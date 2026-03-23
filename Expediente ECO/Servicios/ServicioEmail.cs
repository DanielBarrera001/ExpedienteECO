using System.Net;
using System.Net.Mail;

namespace ExpedienteECO.Servicios
{
    public interface IServicioEmail
    {
        Task EnviarEmailCambioPassword(string receptor, string enlace);
    }

    public class ServicioEmail : IServicioEmail
    {
        private readonly IConfiguration configuration;

        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task EnviarEmailCambioPassword(string receptor, string enlace)
        {
            var email = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

            var cliente = new SmtpClient(host, puerto);
            cliente.EnableSsl = true;
            cliente.UseDefaultCredentials = false;

            cliente.Credentials = new NetworkCredential(email, password);
            var emisor = email;
            var subjetct = "Solicitud cambio de contraseña";
            var contenidoHmtl = $@"Saludos,
               Este mensaje le llega por la solicitud de cambio de contraseña solicitada. Si no fue hecha por usted puede ignorar este mensaje

                Para realizar el cambio de contraseña, haga click en el siguiente enlace:

                {enlace}

                Atentamente,
                Equipo de desarrollo";

            var mensaje = new MailMessage(emisor, receptor, subjetct, contenidoHmtl);
            await cliente.SendMailAsync(mensaje);
        }
    }
}
