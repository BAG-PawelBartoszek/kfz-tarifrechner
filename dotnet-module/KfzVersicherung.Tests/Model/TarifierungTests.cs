using Xunit;
using FluentAssertions;
using DotnetModule.Model;

namespace KfzVersicherung.Tests.Model;

public class TarifierungTest
{
    [Fact(DisplayName = "Berechnung ohne Zielbeitrag, ohne Rabatt und ohne Kasko")]
    public void BerechneTarifierung_KeinZielbeitrag_OhneKasko_KeinRabatt()
    {
        var haftpflicht = new Haftpflicht(75);
        var tarifierung = new Tarifierung(haftpflicht);

        tarifierung.Zielbeitrag.Should().BeNull();
        tarifierung.Gesamtpraemie.Should().Be(75);
        tarifierung.Nettopraemie.Should().Be(75);
    }

    [Fact(DisplayName = "Berechnung ohne Zielbeitrag, ohne Rabatt und mit Kasko")]
    public void BerechneTarifierung_KeinZielbeitrag_MitKasko_KeineRabatte()
    {
        var kasko = new Kasko(50);
        var haftpflicht = new Haftpflicht(75);
        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Zielbeitrag.Should().BeNull();
        tarifierung.Gesamtpraemie.Should().Be(125);
        tarifierung.Nettopraemie.Should().Be(125);
    }

    [Fact(DisplayName = "Berechnung mit Haftpflicht-Rabatt")]
    public void BerechneTarifierung_MitHaftpflichtRabatt()
    {
        var haftpflicht = new Haftpflicht(100, 20); // 20% Rabatt
        var tarifierung = new Tarifierung(haftpflicht);

        tarifierung.Zielbeitrag.Should().BeNull();
        tarifierung.Gesamtpraemie.Should().Be(100);
        tarifierung.Nettopraemie.Should().Be(80); // 100 - 20%
    }

    [Fact(DisplayName = "Berechnung mit Kasko-Rabatt")]
    public void BerechneTarifierung_MitKaskoRabatt()
    {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50, 10); // 10% Rabatt
        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Zielbeitrag.Should().BeNull();
        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().Be(145); // 100 + (50 - 5)
    }

    [Fact(DisplayName = "Berechnung mit beiden Rabatten")]
    public void BerechneTarifierung_MitHaftpflichtUndKaskoRabatt()
    {
        var haftpflicht = new Haftpflicht(100, 20); // 20% Rabatt
        var kasko = new Kasko(50, 10); // 10% Rabatt
        var tarifierung = new Tarifierung(haftpflicht, kasko);

        tarifierung.Zielbeitrag.Should().BeNull();
        tarifierung.Gesamtpraemie.Should().Be(150);
        tarifierung.Nettopraemie.Should().Be(125); // 80 + 45
    }
}
