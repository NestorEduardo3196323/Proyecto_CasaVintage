using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CasaVintage.ViewModels
{
    // Data captured by the salesperson to charge the sale: customer and payment method. The customer
    // name and email are stored for the invoice (repeated customers are allowed, no dedupe). If the
    // method is Card, the card data is also captured: it is a SIMULATION, there is no real charge.
    // Only the last 4 digits are kept; the full number and the CVV are never stored.
    public class CobroViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "The customer name is required.")]
        [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters.")]
        [Display(Name = "Customer name")]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "The customer email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        [StringLength(100, ErrorMessage = "The email cannot exceed 100 characters.")]
        [Display(Name = "Customer email")]
        public string ClienteCorreo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select the payment method.")]
        [Display(Name = "Payment method")]
        public string MetodoPago { get; set; } = string.Empty;

        // ---- Card data (only when MetodoPago == "Tarjeta") ----
        [Display(Name = "Card number")]
        public string? NumeroTarjeta { get; set; }

        [StringLength(100)]
        [Display(Name = "Cardholder")]
        public string? TitularTarjeta { get; set; }

        [Display(Name = "Expiry (MM/YY)")]
        public string? VencimientoTarjeta { get; set; }

        [Display(Name = "CVV")]
        public string? CvvTarjeta { get; set; }

        // Indicates whether the payment is by card (for the view).
        public bool EsTarjeta => string.Equals(MetodoPago, "Tarjeta", StringComparison.Ordinal);

        // The last 4 digits of the captured number (or null). This is the only thing persisted.
        public string? Ultimos4()
        {
            var digitos = SoloDigitos(NumeroTarjeta);
            return digitos.Length >= 4 ? digitos[^4..] : null;
        }

        // Server-side validation: only applies when the method is Card.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!EsTarjeta)
            {
                yield break;
            }

            var numero = SoloDigitos(NumeroTarjeta);
            if (numero.Length != 16)
            {
                yield return new ValidationResult("The card number must have 16 digits.", new[] { nameof(NumeroTarjeta) });
            }
            else if (!LuhnValido(numero))
            {
                yield return new ValidationResult("The card number is not valid.", new[] { nameof(NumeroTarjeta) });
            }

            if (string.IsNullOrWhiteSpace(TitularTarjeta))
            {
                yield return new ValidationResult("The cardholder is required.", new[] { nameof(TitularTarjeta) });
            }

            if (!VencimientoValido(VencimientoTarjeta))
            {
                yield return new ValidationResult("The expiry must be in MM/YY format and not be expired.", new[] { nameof(VencimientoTarjeta) });
            }

            var cvv = SoloDigitos(CvvTarjeta);
            if (cvv.Length is not (3 or 4))
            {
                yield return new ValidationResult("The CVV must have 3 or 4 digits.", new[] { nameof(CvvTarjeta) });
            }
        }

        private static string SoloDigitos(string? valor) =>
            new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

        // Luhn algorithm: validates the check digit of the card number.
        private static bool LuhnValido(string numero)
        {
            var suma = 0;
            var alterna = false;
            for (var i = numero.Length - 1; i >= 0; i--)
            {
                var d = numero[i] - '0';
                if (alterna)
                {
                    d *= 2;
                    if (d > 9)
                    {
                        d -= 9;
                    }
                }
                suma += d;
                alterna = !alterna;
            }
            return suma % 10 == 0;
        }

        // Expiry in MM/YY format, valid month and not before the current month.
        private static bool VencimientoValido(string? vencimiento)
        {
            if (string.IsNullOrWhiteSpace(vencimiento))
            {
                return false;
            }
            var m = Regex.Match(vencimiento.Trim(), @"^(\d{2})/(\d{2})$");
            if (!m.Success)
            {
                return false;
            }
            var mes = int.Parse(m.Groups[1].Value);
            var anio = 2000 + int.Parse(m.Groups[2].Value);
            if (mes < 1 || mes > 12)
            {
                return false;
            }
            var ultimoDia = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));
            return ultimoDia >= DateTime.Today;
        }
    }
}
