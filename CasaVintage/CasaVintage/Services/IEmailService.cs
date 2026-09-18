namespace CasaVintage.Services
{
    // Sending of emails over SMTP (configured in appsettings). It isolates SMTP so that billing
    // only has to ask "send this PDF to this email".
    public interface IEmailService
    {
        // Sends an email with an attached file. Returns true if it was sent, false if it failed (the
        // reason is left in the log). It does not throw exceptions to the caller.
        Task<bool> EnviarConAdjuntoAsync(string destino, string asunto, string cuerpoHtml, byte[] adjunto, string nombreAdjunto);
    }
}
