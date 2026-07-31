namespace CasaVintage.Services
{
    // Envio del comprobante por correo. En este proyecto el envio esta SIMULADO: no se conecta a un
    // servidor SMTP real (para no depender de credenciales externas). Registra la intencion y
    // responde como exito, de modo que el flujo de "enviar por correo" funciona en la demo. Si mas
    // adelante se quisiera envio real, aqui se implementaria la conexion SMTP.
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task<bool> EnviarConAdjuntoAsync(string destino, string asunto, string cuerpoHtml, byte[] adjunto, string nombreAdjunto)
        {
            // Envio simulado: se registra y se responde como exito (no se envia realmente).
            _logger.LogInformation("Envio de comprobante SIMULADO a {Destino} (adjunto {Adjunto}, {Bytes} bytes).",
                destino, nombreAdjunto, adjunto?.Length ?? 0);
            return Task.FromResult(true);
        }
    }
}
