using System.ComponentModel.DataAnnotations;
using DotnetModule.Common;

namespace DotnetModule.Service;

public static class ValidationService
{
    public static void Validate(object obj, string? context = null)
    {
        var validationContext = new ValidationContext(obj);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(obj, validationContext, validationResults, true))
        {
            var fehler = validationResults
                .Where(r => r.ErrorMessage != null)
                .Select(r => FormatErrorMessage(r.ErrorMessage!, r.MemberNames.FirstOrDefault(), context, obj))
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
    
    private static string FormatErrorMessage(string errorMessage, string? memberName, string? context, object obj)
    {
        // Custom handling for Rabatt validation
        if (memberName == "Rabatt" && errorMessage.Contains("zwischen 0 und 99"))
        {
            var rabattProperty = obj.GetType().GetProperty("Rabatt");
            if (rabattProperty != null)
            {
                var rabattValue = (int)rabattProperty.GetValue(obj)!;
            
                string produktName = context ?? obj.GetType().Name;
                produktName = produktName.ToLower() == "haftpflicht" ? "die Haftpflicht" : 
                    produktName.ToLower() == "kasko" ? "die Kasko" : produktName;
            
                if (rabattValue < 0)
                {
                    return $"Rabatt für {produktName} darf nicht negativ sein";
                }
                else if (rabattValue > 99)
                {
                    return $"Rabatt für {produktName} darf nicht größer 99 sein";
                }
            }
        }
    
        // Custom handling for Praemie validation
        if (memberName == "Praemie" && errorMessage.Contains("positiv"))
        {
            string produktName = context ?? obj.GetType().Name;
            produktName = produktName.ToLower() == "haftpflicht" ? "die Haftpflicht" : 
                produktName.ToLower() == "kasko" ? "die Kasko" : produktName;
        
            return $"Prämie für {produktName} muss positiv sein";
        }
    
        // Default formatting
        if (!string.IsNullOrEmpty(context))
        {
            context = context.ToLower() == "haftpflicht" ? "die Haftpflicht" : 
                context.ToLower() == "kasko" ? "die Kasko" : context;
            return $"{errorMessage} für {context}";
        }
    
        return errorMessage;
    }
}
