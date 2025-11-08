using Xunit;
using FluentAssertions;
using DotnetModule.Model;
using DotnetModule.Common;

namespace KfzVersicherung.Tests.Model;

public class TarifierungZielbeitragTest
{
    [Fact(DisplayName = "Zielbeitrag erreicht durch Haftpflicht-Rabatt (Haftpflicht teurer)")]
    public void Zielbeitrag_HaftpflichtTeurer_NurHaftpflichtRabatt()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().BeApproximately(120, 0.5);

        // Haftpflicht sollte rabattiert sein (da teurer), Kasko möglichst nicht
        tarifierung.Haftpflicht.Rabatt.Should().BeGreaterThan(0);

        // Der Gesamtrabatt in % sollte minimal sein
        int gesamtRabattProzent = tarifierung.Haftpflicht.Rabatt 
            + tarifierung.Kasko!.Rabatt;
        gesamtRabattProzent.Should().BeLessThan(40);
    }

    [Fact(DisplayName = "Zielbeitrag erreicht durch Kasko-Rabatt (Kasko teurer)")]
    public void Zielbeitrag_KaskoTeurer_NurKaskoRabatt()
    {
        var haftpflicht = new Haftpflicht(50);
        var kasko = new Kasko(100);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().BeApproximately(120, 0.5);

        // Kasko sollte rabattiert sein (da teurer), Haftpflicht möglichst nicht
        tarifierung.Kasko!.Rabatt.Should().BeGreaterThan(0);

        // Der Gesamtrabatt in % sollte minimal sein
        int gesamtRabattProzent = tarifierung.Haftpflicht.Rabatt 
            + tarifierung.Kasko!.Rabatt;
        gesamtRabattProzent.Should().BeLessThan(40);
    }

    [Fact(DisplayName = "Vergleich: Teurer zuerst spart Prozentpunkte")]
    public void Zielbeitrag_VergleichStrategie()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        // Haftpflicht (teurer) sollte ~30% Rabatt haben
        // Kasko sollte wenig bis gar keinen Rabatt haben
        tarifierung.Haftpflicht.Rabatt.Should().BeInRange(20, 35);
        tarifierung.Kasko!.Rabatt.Should().BeLessThan(10);

        // Gesamtrabatt in Prozentpunkten sollte ~30 sein, nicht ~60
        int gesamtRabattProzent = tarifierung.Haftpflicht.Rabatt 
            + tarifierung.Kasko!.Rabatt;
        gesamtRabattProzent.Should().BeLessThan(40);
    }

    [Fact(DisplayName = "Zielbeitrag mit gleichen Prämien")]
    public void Zielbeitrag_GleichePraemien()
    {
        var haftpflicht = new Haftpflicht(75);
        var kasko = new Kasko(75);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().BeApproximately(120, 0.5);

        // Bei gleichen Prämien ist es egal - Haftpflicht wird bevorzugt
        tarifierung.Haftpflicht.Rabatt.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "Zielbeitrag erfordert beide Rabatte (Haftpflicht teurer)")]
    public void Zielbeitrag_BeideRabatte_HaftpflichtTeurer()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(60, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().BeApproximately(60, 1.0);

        // Haftpflicht sollte sehr stark rabattiert sein (da teurer)
        tarifierung.Haftpflicht.Rabatt.Should().BeGreaterThan(70);

        // Die Nettosumme muss stimmen
        double nettoGesamt = tarifierung.Haftpflicht.GetPraemieMitRabatt() 
            + tarifierung.Kasko!.GetPraemieMitRabatt();
        nettoGesamt.Should().BeApproximately(60, 1.0);
    }

    [Fact(DisplayName = "Zielbeitrag erfordert beide Rabatte (Kasko teurer)")]
    public void Zielbeitrag_BeideRabatte_KaskoTeurer()
    {
        var haftpflicht = new Haftpflicht(50);
        var kasko = new Kasko(100);
        var tarifierung = new Tarifierung(60, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().BeApproximately(60, 1.0);

        // Kasko (teurer) wird zuerst rabattiert
        tarifierung.Kasko!.Rabatt.Should().BeGreaterThan(80);

        // Die Nettosumme muss stimmen
        double nettoGesamt = tarifierung.Haftpflicht.GetPraemieMitRabatt() 
            + tarifierung.Kasko!.GetPraemieMitRabatt();
        nettoGesamt.Should().BeApproximately(60, 1.0);
    }

    [Fact(DisplayName = "Zielbeitrag gleich Gesamtprämie - kein Rabatt nötig")]
    public void Zielbeitrag_GleichGesamtpraemie()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(150, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().Be(150);
        tarifierung.Haftpflicht.Rabatt.Should().Be(0);
        tarifierung.Kasko!.Rabatt.Should().Be(0);
    }

    [Fact(DisplayName = "Zielbeitrag höher als Gesamtprämie - kein Rabatt nötig")]
    public void Zielbeitrag_HoeherAlsGesamtpraemie()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(200, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().Be(150);
        tarifierung.Haftpflicht.Rabatt.Should().Be(0);
        tarifierung.Kasko!.Rabatt.Should().Be(0);
    }

    [Fact(DisplayName = "Zielbeitrag nur mit Haftpflicht (ohne Kasko)")]
    public void Zielbeitrag_NurHaftpflicht()
    {
        var haftpflicht = new Haftpflicht(100);
        var tarifierung = new Tarifierung(80, haftpflicht, null);

        tarifierung.Gesamtpraemie.Should().Be(100);
        tarifierung.Nettopraemie.Should().BeApproximately(80, 0.5);
        tarifierung.Haftpflicht.Rabatt.Should().BeInRange(15, 25);
    }

    [Fact(DisplayName = "Zielbeitrag nicht erreichbar - Exception")]
    public void Zielbeitrag_NichtErreichbar()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);

        // Minimaler Beitrag bei 99% Rabatt: 150 * 0.01 = 1.5
        var act = () => new Tarifierung(1.0, haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*nicht erreichbar*")
            .WithMessage("*Minimaler Beitrag*");
    }

    [Fact(DisplayName = "Sehr niedriger Zielbeitrag (aber erreichbar)")]
    public void Zielbeitrag_SehrNiedrig()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(2.0, haftpflicht, kasko);

        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().BeApproximately(2.0, 0.5);

        // Beide sollten nahe am maximalen Rabatt sein
        tarifierung.Haftpflicht.Rabatt.Should().BeGreaterThanOrEqualTo(98);
        tarifierung.Kasko!.Rabatt.Should().BeGreaterThanOrEqualTo(98);
    }

    [Fact(DisplayName = "Optimierung minimiert Prozent-Rabatt (Haftpflicht teurer)")]
    public void Zielbeitrag_MinimalerProzentRabatt_HaftpflichtTeurer()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(20);
        var tarifierung = new Tarifierung(110, haftpflicht, kasko);

        // 10€ Rabatt nötig: Haftpflicht mit 10% = besser als Kasko mit 50%
        tarifierung.Haftpflicht.Rabatt.Should().BeGreaterThan(0);
        tarifierung.Kasko!.Rabatt.Should().BeLessThanOrEqualTo(5);

        int gesamtRabattProzent = tarifierung.Haftpflicht.Rabatt 
            + tarifierung.Kasko!.Rabatt;
        gesamtRabattProzent.Should().BeLessThan(20);
    }

    [Fact(DisplayName = "Optimierung minimiert Prozent-Rabatt (Kasko teurer)")]
    public void Zielbeitrag_MinimalerProzentRabatt_KaskoTeurer()
    {
        var haftpflicht = new Haftpflicht(20);
        var kasko = new Kasko(100);
        var tarifierung = new Tarifierung(110, haftpflicht, kasko);

        // 10€ Rabatt nötig: Kasko mit 10% = besser als Haftpflicht mit 50%
        tarifierung.Kasko!.Rabatt.Should().BeGreaterThan(0);
        tarifierung.Haftpflicht.Rabatt.Should().BeLessThanOrEqualTo(5);

        int gesamtRabattProzent = tarifierung.Haftpflicht.Rabatt 
            + tarifierung.Kasko!.Rabatt;
        gesamtRabattProzent.Should().BeLessThan(20);
    }
}
