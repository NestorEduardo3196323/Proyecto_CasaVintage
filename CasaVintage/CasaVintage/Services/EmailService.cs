namespace CasaVintage.Services
{
    // Sending the receipt by email. In this project the sending is SIMULATED: it does not connect to
    // a real SMTP server (to avoid depending on external credentials). It logs the intent and
    // responds as success, so the "send by email" flow works in the demo. If real sending were
    // wanted later, the SMTP connection would be implemented here.
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task<bool> EnviarConAdjuntoAsync(string destino, string asunto, string cuerpoHtml, byte[] adjunto, string nombreAdjunto)
        {
            // Simulated sending: it is logged and responded as success (nothing is actually sent).
            _logger.LogInformation("SIMULATED receipt sending to {Destino} (attachment {Adjunto}, {Bytes} bytes).",
                destino, nombreAdjunto, adjunto?.Length ?? 0);
            return Task.FromResult(true);
        }
    }
}
