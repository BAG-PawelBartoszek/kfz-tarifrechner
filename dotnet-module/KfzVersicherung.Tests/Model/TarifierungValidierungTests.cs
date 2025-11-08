using Xunit;
using FluentAssertions;
using DotnetModule.Model;
using DotnetModule.Common;

namespace KfzVersicherung.Tests.Model;

public class TarifierungValidierungTest
{
    [Fact(DisplayName = "Haftpflicht null - sollte PlausiException werfen")]
    public void Validierung_HaftpflichtNull_WirftException()
    {
        var act = () => new Tarifierung(null!);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Haftpflicht ist Pflichtfeld*");
    }

    [Fact(DisplayName = "Haftpflicht Prämie negativ - sollte PlausiException werfen")]
    public void Validierung_HaftpflichtPraemieNegativ_WirftException()
    {
        var haftpflicht = new Haftpflicht(-100);

        var act = () => new Tarifierung(haftpflicht);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Prämie für die Haftpflicht muss positiv sein*");
    }

    [Fact(DisplayName = "Haftpflicht Prämie null (0) - sollte PlausiException werfen")]
    public void Validierung_HaftpflichtPraemieNull_WirftException()
    {
        var haftpflicht = new Haftpflicht(0);

        var act = () => new Tarifierung(haftpflicht);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Prämie für die Haftpflicht muss positiv sein*");
    }

    [Fact(DisplayName = "Haftpflicht Rabatt negativ - sollte PlausiException werfen")]
    public void Validierung_HaftpflichtRabattNegativ_WirftException()
    {
        var haftpflicht = new Haftpflicht(100, -10);

        var act = () => new Tarifierung(haftpflicht);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Rabatt für die Haftpflicht darf nicht negativ sein*");
    }

    [Fact(DisplayName = "Haftpflicht Rabatt >= 100 - sollte PlausiException werfen")]
    public void Validierung_HaftpflichtRabatt100_WirftException()
    {
        var haftpflicht = new Haftpflicht(100, 100);

        var act = () => new Tarifierung(haftpflicht);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Rabatt für die Haftpflicht darf nicht größer 99 sein*");
    }

    [Fact(DisplayName = "Haftpflicht Rabatt > 100 - sollte PlausiException werfen")]
    public void Validierung_HaftpflichtRabattUeber100_WirftException()
    {
        var haftpflicht = new Haftpflicht(100, 150);

        var act = () => new Tarifierung(haftpflicht);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Rabatt für die Haftpflicht darf nicht größer 99 sein*");
    }

    [Fact(DisplayName = "Kasko Prämie negativ - sollte PlausiException werfen")]
    public void Validierung_KaskoPraemieNegativ_WirftException()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(-50);

        var act = () => new Tarifierung(haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Prämie für die Kasko muss positiv sein*");
    }

    [Fact(DisplayName = "Kasko Prämie null (0) - sollte PlausiException werfen")]
    public void Validierung_KaskoPraemieNull_WirftException()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(0);

        var act = () => new Tarifierung(haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Prämie für die Kasko muss positiv sein*");
    }

    [Fact(DisplayName = "Kasko Rabatt negativ - sollte PlausiException werfen")]
    public void Validierung_KaskoRabattNegativ_WirftException()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50, -10);

        var act = () => new Tarifierung(haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Rabatt für die Kasko darf nicht negativ sein*");
    }

    [Fact(DisplayName = "Kasko Rabatt >= 100 - sollte PlausiException werfen")]
    public void Validierung_KaskoRabatt100_WirftException()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50, 100);

        var act = () => new Tarifierung(haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Rabatt für die Kasko darf nicht größer 99 sein*");
    }

    [Fact(DisplayName = "Zielbeitrag negativ - sollte PlausiException werfen")]
    public void Validierung_ZielbeitragNegativ_WirftException()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);

        var act = () => new Tarifierung(-200, haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Zielbeitrag von -200,00€ ist nicht erreichbar*");
    }

    [Fact(DisplayName = "Zielbeitrag null (0) - sollte PlausiException werfen")]
    public void Validierung_ZielbeitragNull_WirftException()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);

        var act = () => new Tarifierung(0, haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .WithMessage("*Zielbeitrag von 0,00€ ist nicht erreichbar*");
    }

    [Fact(DisplayName = "Multiple Fehler - sollte alle Fehler in PlausiException sammeln")]
    public void Validierung_MultipleFehler_SammeltAlleFehler()
    {
        var haftpflicht = new Haftpflicht(-100, -10);

        var act = () => new Tarifierung(haftpflicht);

        act.Should().Throw<PlausiException>()
            .Which.Fehler.Should().HaveCount(2)
            .And.Contain("Prämie für die Haftpflicht muss positiv sein")
            .And.Contain("Rabatt für die Haftpflicht darf nicht negativ sein");
    }

    [Fact(DisplayName = "Haftpflicht und Kasko mit mehreren Fehlern")]
    public void Validierung_HaftpflichtUndKaskoMultipleFehler_SammeltAlleFehler()
    {
        var haftpflicht = new Haftpflicht(-100, 150);
        var kasko = new Kasko(-50, -20);

        var act = () => new Tarifierung(haftpflicht, kasko);

        act.Should().Throw<PlausiException>()
            .Which.Fehler.Should().HaveCount(4)
            .And.Contain("Prämie für die Haftpflicht muss positiv sein")
            .And.Contain("Rabatt für die Haftpflicht darf nicht größer 99 sein")
            .And.Contain("Prämie für die Kasko muss positiv sein")
            .And.Contain("Rabatt für die Kasko darf nicht negativ sein");
    }

    [Fact(DisplayName = "Gültige Haftpflicht ohne Kasko - keine Exception")]
    public void Validierung_GueltigeHaftpflichtOhneKasko_KeineException()
    {
        var haftpflicht = new Haftpflicht(100, 10);

        var tarifierung = new Tarifierung(haftpflicht);

        tarifierung.Should().NotBeNull();
        tarifierung.Haftpflicht.Should().Be(haftpflicht);
        tarifierung.Kasko.Should().BeNull();
    }

    [Fact(DisplayName = "Gültige Haftpflicht mit Kasko - keine Exception")]
    public void Validierung_GueltigeHaftpflichtMitKasko_KeineException()
    {
        var haftpflicht = new Haftpflicht(100, 10);
        var kasko = new Kasko(50, 15);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Should().NotBeNull();
        tarifierung.Haftpflicht.Should().Be(haftpflicht);
        tarifierung.Kasko.Should().Be(kasko);
    }

    [Fact(DisplayName = "Gültiger Zielbeitrag - keine Exception")]
    public void Validierung_GueltigerZielbeitrag_KeineException()
    {
        var haftpflicht = new Haftpflicht(100, 10);
        var kasko = new Kasko(50, 15);

        var tarifierung = new Tarifierung(200, haftpflicht, kasko);

        tarifierung.Should().NotBeNull();
        tarifierung.Zielbeitrag.Should().Be(200);
    }

    [Fact(DisplayName = "Rabatt 0 ist gültig")]
    public void Validierung_RabattNull_IstGueltig()
    {
        var haftpflicht = new Haftpflicht(100, 0);
        var kasko = new Kasko(50, 0);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Should().NotBeNull();
        tarifierung.Haftpflicht.Rabatt.Should().Be(0);
        tarifierung.Kasko!.Rabatt.Should().Be(0);
    }

    [Fact(DisplayName = "Rabatt 99 ist gültig (Grenzwert)")]
    public void Validierung_Rabatt99_IstGueltig()
    {
        var haftpflicht = new Haftpflicht(100, 99);
        var kasko = new Kasko(50, 99);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Should().NotBeNull();
        tarifierung.Haftpflicht.Rabatt.Should().Be(99);
        tarifierung.Kasko!.Rabatt.Should().Be(99);
    }

    [Fact(DisplayName = "Sehr kleine Prämie (0.01) ist gültig")]
    public void Validierung_SehrKleinePraemie_IstGueltig()
    {
        var haftpflicht = new Haftpflicht(0.01, 0);
        var kasko = new Kasko(0.01, 0);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Should().NotBeNull();
        tarifierung.Haftpflicht.Praemie.Should().Be(0.01);
        tarifierung.Kasko!.Praemie.Should().Be(0.01);
    }
}
