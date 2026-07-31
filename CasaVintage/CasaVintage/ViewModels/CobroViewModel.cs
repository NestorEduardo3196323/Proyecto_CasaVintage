using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CasaVintage.ViewModels
{
    // Datos que captura el vendedor para cobrar la venta: cliente y metodo de pago. El nombre y el
    // correo del cliente se guardan para la factura (se permiten clientes repetidos, no se dedupea).
    // Si el metodo es Tarjeta, se capturan tambien los datos de la tarjeta: es una SIMULACION, no hay
    // cobro real. Solo se conservan los ultimos 4 digitos; el numero completo y el CVV nunca se guardan.
    public class CobroViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        [Display(Name = "Nombre del cliente")]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo del cliente es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
        [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
        [Display(Name = "Correo del cliente")]
        public string ClienteCorreo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona el metodo de pago.")]
        [Display(Name = "Metodo de pago")]
        public string MetodoPago { get; set; } = string.Empty;

        // ---- Datos de la tarjeta (solo cuando MetodoPago == "Tarjeta") ----
        [Display(Name = "Numero de tarjeta")]
        public string? NumeroTarjeta { get; set; }

        [StringLength(100)]
        [Display(Name = "Titular de la tarjeta")]
        public string? TitularTarjeta { get; set; }

        [Display(Name = "Vencimiento (MM/AA)")]
        public string? VencimientoTarjeta { get; set; }

        [Display(Name = "CVV")]
        public string? CvvTarjeta { get; set; }

        // Indica si el pago es con tarjeta (para la vista).
        public bool EsTarjeta => string.Equals(MetodoPago, "Tarjeta", StringComparison.Ordinal);

        // Los ultimos 4 digitos del numero capturado (o null). Es lo unico que se persiste.
        public string? Ultimos4()
        {
            var digitos = SoloDigitos(NumeroTarjeta);
            return digitos.Length >= 4 ? digitos[^4..] : null;
        }

        // Validacion del lado del servidor: solo aplica cuando el metodo es Tarjeta.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!EsTarjeta)
            {
                yield break;
            }

            var numero = SoloDigitos(NumeroTarjeta);
            if (numero.Length != 16)
            {
                yield return new ValidationResult("El numero de tarjeta debe tener 16 digitos.", new[] { nameof(NumeroTarjeta) });
            }
            else if (!LuhnValido(numero))
            {
                yield return new ValidationResult("El numero de tarjeta no es valido.", new[] { nameof(NumeroTarjeta) });
            }

            if (string.IsNullOrWhiteSpace(TitularTarjeta))
            {
                yield return new ValidationResult("El titular de la tarjeta es obligatorio.", new[] { nameof(TitularTarjeta) });
            }

            if (!VencimientoValido(VencimientoTarjeta))
            {
                yield return new ValidationResult("El vencimiento debe tener formato MM/AA y no estar vencido.", new[] { nameof(VencimientoTarjeta) });
            }

            var cvv = SoloDigitos(CvvTarjeta);
            if (cvv.Length is not (3 or 4))
            {
                yield return new ValidationResult("El CVV debe tener 3 o 4 digitos.", new[] { nameof(CvvTarjeta) });
            }
        }

        private static string SoloDigitos(string? valor) =>
            new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

        // Algoritmo de Luhn: valida el digito verificador del numero de tarjeta.
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

        // Vencimiento en formato MM/AA, mes valido y no anterior al mes actual.
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
