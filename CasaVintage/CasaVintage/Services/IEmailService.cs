namespace CasaVintage.Services
{
    // Envio de correos por SMTP (configurado en appsettings). Aisla el SMTP para que la
    // facturacion solo pida "envia este PDF a este correo".
    public interface IEmailService
    {
        // Envia un correo con un archivo adjunto. Devuelve true si se envio, false si fallo (el
        // motivo queda en el log). No lanza excepciones al llamador.
        Task<bool> EnviarConAdjuntoAsync(string destino, string asunto, string cuerpoHtml, byte[] adjunto, string nombreAdjunto);
    }
}
