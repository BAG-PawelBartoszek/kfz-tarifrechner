
using DotnetModule.Model;
using DotnetModule.Common;

Console.WriteLine("=== KFZ-Versicherung Tarifierung ===\n");

try
{
    // Beispiel 1: Einfache Tarifierung nur mit Haftpflicht
    Console.WriteLine("Beispiel 1: Nur Haftpflicht");
    var haftpflicht1 = new Haftpflicht(500.0);
    var tarif1 = new Tarifierung(haftpflicht1);
    
    Console.WriteLine($"Gesamtprämie: {tarif1.Gesamtpraemie:F2}€");
    Console.WriteLine($"Nettoprämie: {tarif1.Nettopraemie:F2}€");
    Console.WriteLine($"Haftpflicht Rabatt: {tarif1.Haftpflicht.Rabatt}%\n");

    // Beispiel 2: Tarifierung mit Haftpflicht und Kasko
    Console.WriteLine("Beispiel 2: Haftpflicht + Kasko");
    var haftpflicht2 = new Haftpflicht(500.0);
    var kasko2 = new Kasko(300.0);
    var tarif2 = new Tarifierung(haftpflicht2, kasko2);
    
    Console.WriteLine($"Gesamtprämie: {tarif2.Gesamtpraemie:F2}€");
    Console.WriteLine($"Nettoprämie: {tarif2.Nettopraemie:F2}€");
    Console.WriteLine($"Haftpflicht Rabatt: {tarif2.Haftpflicht.Rabatt}%");
    Console.WriteLine($"Kasko Rabatt: {tarif2.Kasko?.Rabatt}%\n");

    // Beispiel 3: Tarifierung mit Zielbeitrag
    Console.WriteLine("Beispiel 3: Mit Zielbeitrag von 600€");
    var haftpflicht3 = new Haftpflicht(500.0);
    var kasko3 = new Kasko(300.0);
    var tarif3 = new Tarifierung(600.0, haftpflicht3, kasko3);
    
    Console.WriteLine($"Zielbeitrag: {tarif3.Zielbeitrag:F2}€");
    Console.WriteLine($"Gesamtprämie: {tarif3.Gesamtpraemie:F2}€");
    Console.WriteLine($"Nettoprämie: {tarif3.Nettopraemie:F2}€");
    Console.WriteLine($"Haftpflicht Rabatt: {tarif3.Haftpflicht.Rabatt}%");
    Console.WriteLine($"Kasko Rabatt: {tarif3.Kasko?.Rabatt}%\n");

    // Beispiel 4: Validierungsfehler - negative Prämie
    Console.WriteLine("Beispiel 4: Validierungsfehler (wird Exception werfen)");
    var haftpflichtInvalid = new Haftpflicht(-100.0);
    var tarifInvalid = new Tarifierung(haftpflichtInvalid);
}
catch (PlausiException ex)
{
    Console.WriteLine("\nErwarteter Fehler aufgetreten (siehe oben)");
}

Console.WriteLine("\n=== Programm beendet ===");
