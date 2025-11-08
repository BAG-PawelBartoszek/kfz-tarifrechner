using System.ComponentModel.DataAnnotations;
using DotnetModule.Common;

namespace DotnetModule.Service;

public static class ValidationService
{
    public static void Validate<T>(T obj, string? propertyPrefix = null)
    {
        var context = new ValidationContext(obj);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(obj, context, results, true))
        {
            var fehler = results
                .Select(r => FormatFehler(r, propertyPrefix))
                .ToList();

            throw new PlausiException(fehler);
        }
    }

    private static string FormatFehler(ValidationResult result, string? propertyPrefix)
    {
        var message = result.ErrorMessage ?? "Validierungsfehler";
        
        if (propertyPrefix != null)
        {
            return propertyPrefix switch
            {
                "haftpflicht" => FormatHaftpflichtFehler(message),
                "kasko" => FormatKaskoFehler(message),
                _ => message
            };
        }

        return message;
    }

    private static string FormatHaftpflichtFehler(string message)
    {
        if (message.Contains("Prämie"))
            return "Prämie für die Haftpflicht muss positiv sein";
        if (message.Contains("Rabatt") && message.Contains("negativ"))
            return "Rabatt für die Haftpflicht darf nicht negativ sein";
        if (message.Contains("Rabatt"))
            return "Rabatt für die Haftpflicht darf nicht größer 99 sein";
        
        return message;
    }

    private static string FormatKaskoFehler(string message)
    {
        if (message.Contains("Prämie"))
            return "Prämie für die Kasko muss positiv sein";
        if (message.Contains("Rabatt") && message.Contains("negativ"))
            return "Rabatt für die Kasko darf nicht negativ sein";
        if (message.Contains("Rabatt"))
            return "Rabatt für die Kasko darf nicht größer 99 sein";
        
        return message;
    }
}
