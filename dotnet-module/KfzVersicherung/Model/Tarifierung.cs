
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
        if (Zielbeitrag == null)
        {
            return;
        }

        double aktuelleGesamtpraemie = Haftpflicht.Praemie + (Kasko?.Praemie ?? 0.0);

        // Prüfen, ob Zielbeitrag überhaupt erreichbar ist
        if (Zielbeitrag >= aktuelleGesamtpraemie)
        {
            // Kein Rabatt nötig, Zielbeitrag ist höher als Gesamtprämie
            return;
        }

        // Minimaler erreichbarer Beitrag bei maximalem Rabatt (99%)
        double minimalerBeitrag = aktuelleGesamtpraemie * 0.01; // 1% der Gesamtprämie

        if (Zielbeitrag < minimalerBeitrag)
        {
            throw new PlausiException(new List<string>
            {
                $"Zielbeitrag von {Zielbeitrag:F2}€ ist nicht erreichbar. " +
                $"Minimaler Beitrag bei maximalem Rabatt (99%): {minimalerBeitrag:F2}€"
            });
        }

        // Strategie: Das TEURERE Produkt zuerst rabattieren
        double benoetigterRabatt = aktuelleGesamtpraemie - Zielbeitrag.Value;

        if (Kasko != null)
        {
            // Bestimme, welches Produkt teurer ist
            bool kaskoIstTeurer = Kasko.Praemie > Haftpflicht.Praemie;

            if (kaskoIstTeurer)
            {
                VerteileRabatt(benoetigterRabatt, Kasko, Haftpflicht);
            }
            else
            {
                VerteileRabatt(benoetigterRabatt, Haftpflicht, Kasko);
            }
        }
        else
        {
            // Nur Haftpflicht vorhanden, gesamter Rabatt auf Haftpflicht
            int haftpflichtRabattProzent = BerechneRabattProzent(benoetigterRabatt, Haftpflicht.Praemie);
            Haftpflicht.Rabatt = haftpflichtRabattProzent;
        }

        // Feinabstimmung: Iterativ optimieren, um genau den Zielbeitrag zu treffen
        OptimiereRabatte();
    }

    private int BerechneRabattProzent(double benoetigterBetrag, double praemie)
    {
        if (praemie == 0) return 0;
        int prozent = (int)Math.Ceiling((benoetigterBetrag / praemie) * 100);
        return Math.Min(prozent, 99);
    }

    private void VerteileRabatt(double benoetigterRabatt, Produkt teureres, Produkt guenstigeres)
    {
        double maxTeureresRabatt = teureres.Praemie * 0.99; // 99% Rabatt möglich

        if (benoetigterRabatt <= maxTeureresRabatt)
        {
            // Rabatt kann komplett über das teurere Produkt erreicht werden
            int rabattProzent = BerechneRabattProzent(benoetigterRabatt, teureres.Praemie);
            teureres.Rabatt = rabattProzent;
            guenstigeres.Rabatt = 0; // Explizit auf 0 setzen
        }
        else
        {
            // Maximaler Rabatt auf teureres, Rest auf günstigeres
            teureres.Rabatt = 99;
            double verbleibendenRabatt = benoetigterRabatt - maxTeureresRabatt;
            int guenstigeresProzent = BerechneRabattProzent(verbleibendenRabatt, guenstigeres.Praemie);
            guenstigeres.Rabatt = guenstigeresProzent;
        }
    }

    private void OptimiereRabatte()
    {
        const double toleranz = 0.01; // 1 Cent Toleranz
        const int maxIterationen = 100;

        for (int i = 0; i < maxIterationen; i++)
        {
            double aktuellerNettobeitrag = BerechneAktuellenNettobeitrag();
            double differenz = aktuellerNettobeitrag - Zielbeitrag!.Value;

            // Ziel erreicht (mit Toleranz)
            if (Math.Abs(differenz) < toleranz)
            {
                break;
            }

            // Zu teuer - mehr Rabatt nötig
            if (differenz > 0)
            {
                if (!ErhoeheRabatt())
                {
                    break; // Maximaler Rabatt erreicht
                }
            }
            // Zu günstig - Rabatt reduzieren
            else
            {
                if (!ReduziereRabatt())
                {
                    break; // Minimaler Rabatt erreicht
                }
            }
        }
    }

    private double BerechneAktuellenNettobeitrag()
    {
        double nettoHaftpflicht = Haftpflicht.GetPraemieMitRabatt();
        double nettoKasko = Kasko?.GetPraemieMitRabatt() ?? 0.0;
        return nettoHaftpflicht + nettoKasko;
    }

    private bool ErhoeheRabatt()
    {
        // Priorität: Das TEURERE Produkt zuerst (minimiert Prozent-Rabatt)
        if (Kasko != null)
        {
            bool kaskoIstTeurer = Kasko.Praemie > Haftpflicht.Praemie;

            if (kaskoIstTeurer)
            {
                if (Kasko.Rabatt < 99)
                {
                    Kasko.Rabatt = Math.Min(Kasko.Rabatt + 1, 99);
                    return true;
                }
                if (Haftpflicht.Rabatt < 99)
                {
                    Haftpflicht.Rabatt = Math.Min(Haftpflicht.Rabatt + 1, 99);
                    return true;
                }
            }
            else
            {
                if (Haftpflicht.Rabatt < 99)
                {
                    Haftpflicht.Rabatt = Math.Min(Haftpflicht.Rabatt + 1, 99);
                    return true;
                }
                if (Kasko.Rabatt < 99)
                {
                    Kasko.Rabatt = Math.Min(Kasko.Rabatt + 1, 99);
                    return true;
                }
            }
        }
        else
        {
            if (Haftpflicht.Rabatt < 99)
            {
                Haftpflicht.Rabatt = Math.Min(Haftpflicht.Rabatt + 1, 99);
                return true;
            }
        }
        return false;
    }

    private bool ReduziereRabatt()
    {
        // Priorität: Das GÜNSTIGERE Produkt zuerst reduzieren (umgekehrte Reihenfolge)
        if (Kasko != null)
        {
            bool kaskoIstTeurer = Kasko.Praemie > Haftpflicht.Praemie;

            if (kaskoIstTeurer)
            {
                // Haftpflicht ist günstiger → erst Haftpflicht reduzieren
                if (Haftpflicht.Rabatt > 0)
                {
                    Haftpflicht.Rabatt = Math.Max(Haftpflicht.Rabatt - 1, 0);
                    return true;
                }
                if (Kasko.Rabatt > 0)
                {
                    Kasko.Rabatt = Math.Max(Kasko.Rabatt - 1, 0);
                    return true;
                }
            }
            else
            {
                // Kasko ist günstiger oder gleich → erst Kasko reduzieren
                if (Kasko.Rabatt > 0)
                {
                    Kasko.Rabatt = Math.Max(Kasko.Rabatt - 1, 0);
                    return true;
                }
                if (Haftpflicht.Rabatt > 0)
                {
                    Haftpflicht.Rabatt = Math.Max(Haftpflicht.Rabatt - 1, 0);
                    return true;
                }
            }
        }
        else
        {
            if (Haftpflicht.Rabatt > 0)
            {
                Haftpflicht.Rabatt = Math.Max(Haftpflicht.Rabatt - 1, 0);
                return true;
            }
        }
        return false;
    }
}
