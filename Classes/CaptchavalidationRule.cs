using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp
{
    public class CaptchavalidationRule : ValidationRule
    {
        public Binding ValidCaptcha {  get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? "").ToString().Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "Капча обязательна");
            if (input != ValidCaptcha.ToString())
                return new ValidationResult(false, "Неверная капча");

            return ValidationResult.ValidResult;
        }
    }
}
