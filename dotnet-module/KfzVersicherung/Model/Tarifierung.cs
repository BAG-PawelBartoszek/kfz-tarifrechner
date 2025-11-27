
using System.ComponentModel.DataAnnotations;
using DotnetModule.Common;
using DotnetModule.Service;

namespace DotnetModule.Model;

public class Tarifierung
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Zielbeitrag muss positiv sein")]
    public double? Zielbeitrag { get; }

    public double Gesamtpraemie { get; private set; }
    public double Nettopraemie { get; private set; }

    [Required(ErrorMessage = "Haftpflicht ist Pflichtfeld")]
    public Haftpflicht Haftpflicht { get; }

    public Kasko? Kasko { get; }

    public Tarifierung(Haftpflicht haftpflicht)
    {
        Haftpflicht = haftpflicht;
        BerechneTarifierung();
    }

    public Tarifierung(Haftpflicht haftpflicht, Kasko? kasko)
    {
        Haftpflicht = haftpflicht;
        Kasko = kasko;
        BerechneTarifierung();
    }

    public Tarifierung(double zielbeitrag, Haftpflicht haftpflicht, Kasko? kasko)
    {
        Haftpflicht = haftpflicht;
        Kasko = kasko;
        Zielbeitrag = zielbeitrag;
        BerechneRabatteFuerZielbeitrag();
        BerechneTarifierung();
    }

    private void BerechneTarifierung()
    {
        ValidateAll();
        Gesamtpraemie = BerechneGesamtpraemie();
        Nettopraemie = BerechneNettopraemie();
    }

    private void ValidateAll()
    {
        var allFehler = new List<string>();

        // Validate that Haftpflicht is not null first
        if (Haftpflicht == null)
        {
            allFehler.Add("Haftpflicht ist Pflichtfeld");
        }

        // Collect all validation errors without throwing immediately
        try
        {
            ValidationService.Validate(this);
        }
        catch (PlausiException ex)
        {
            allFehler.AddRange(ex.Fehler);
        }

        // Only validate Haftpflicht if it's not null
        if (Haftpflicht != null)
        {
            try
            {
                ValidationService.Validate(Haftpflicht, "haftpflicht");
            }
            catch (PlausiException ex)
            {
                allFehler.AddRange(ex.Fehler);
            }
        }

        if (Kasko != null)
        {
            try
            {
                ValidationService.Validate(Kasko, "kasko");
            }
            catch (PlausiException ex)
            {
                allFehler.AddRange(ex.Fehler);
            }
        }

        // If any errors were collected, throw them all at once
        if (allFehler.Count > 0)
        {
            throw new PlausiException(allFehler);
        }
    }

    private double BerechneNettopraemie()
    {
        var nettoHaftpflicht = Haftpflicht.GetPraemieMitRabatt();
        var nettoKasko = Kasko?.GetPraemieMitRabatt() ?? 0.0;
        return nettoHaftpflicht + nettoKasko;
    }

    private double BerechneGesamtpraemie()
    {
        double haftpflichtPraemie = Haftpflicht.Praemie;
        double kaskoPraemie = Kasko?.Praemie ?? 0.0;
        return haftpflichtPraemie + kaskoPraemie;
    }

    private void BerechneRabatteFuerZielbeitrag()
    {
        // ToDo: implementiere mich!!!
    }
}