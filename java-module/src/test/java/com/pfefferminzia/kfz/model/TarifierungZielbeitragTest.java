
package com.pfefferminzia.kfz.model;

import com.pfefferminzia.kfz.common.PlausiException;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.assertj.core.api.Assertions.within;

@DisplayName("Tarifierung Zielbeitrag Berechnungs Tests")
class TarifierungZielbeitragTest {

    @Test
    @DisplayName("Zielbeitrag erreicht durch Haftpflicht-Rabatt (Haftpflicht teurer)")
    void zielbeitrag_haftpflichtTeurer_nurHaftpflichtRabatt() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(120, within(0.5));
        
        // Haftpflicht sollte rabattiert sein (da teurer), Kasko möglichst nicht
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isGreaterThan(0);
        
        // Der Gesamtrabatt in % sollte minimal sein
        int gesamtRabattProzent = tarifierung.getHaftpflicht().getRabatt() 
            + tarifierung.getKasko().getRabatt();
        assertThat(gesamtRabattProzent).isLessThan(40); // Deutlich weniger als 60% wenn Kasko rabattiert würde
    }

    @Test
    @DisplayName("Zielbeitrag erreicht durch Kasko-Rabatt (Kasko teurer)")
    void zielbeitrag_kaskoTeurer_nurKaskoRabatt() {
        var haftpflicht = new Haftpflicht(50);
        var kasko = new Kasko(100);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(120, within(0.5));
        
        // Kasko sollte rabattiert sein (da teurer), Haftpflicht möglichst nicht
        assertThat(tarifierung.getKasko().getRabatt()).isGreaterThan(0);
        
        // Der Gesamtrabatt in % sollte minimal sein
        int gesamtRabattProzent = tarifierung.getHaftpflicht().getRabatt() 
            + tarifierung.getKasko().getRabatt();
        assertThat(gesamtRabattProzent).isLessThan(40); // Deutlich weniger als 60% wenn Haftpflicht rabattiert würde
    }

    @Test
    @DisplayName("Vergleich: Teurer zuerst spart Prozentpunkte")
    void zielbeitrag_vergleichStrategie() {
        // Szenario: Haftpflicht 100€, Kasko 50€, Ziel 120€ (30€ Rabatt nötig)
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        // Haftpflicht (teurer) sollte ~30% Rabatt haben
        // Kasko sollte wenig bis gar keinen Rabatt haben
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isBetween(20, 35);
        assertThat(tarifierung.getKasko().getRabatt()).isLessThan(10);
        
        // Gesamtrabatt in Prozentpunkten sollte ~30 sein, nicht ~60
        int gesamtRabattProzent = tarifierung.getHaftpflicht().getRabatt() 
            + tarifierung.getKasko().getRabatt();
        assertThat(gesamtRabattProzent).isLessThan(40);
    }

    @Test
    @DisplayName("Zielbeitrag mit gleichen Prämien")
    void zielbeitrag_gleichePraemien() {
        var haftpflicht = new Haftpflicht(75);
        var kasko = new Kasko(75);
        var tarifierung = new Tarifierung(120, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(120, within(0.5));
        
        // Bei gleichen Prämien ist es egal - Haftpflicht wird bevorzugt (nicht >)
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isGreaterThan(0);
    }

    @Test
    @DisplayName("Zielbeitrag erfordert beide Rabatte (Haftpflicht teurer)")
    void zielbeitrag_beideRabatte_haftpflichtTeurer() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(60, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(60, within(1.0));
    
        // Mathematik: 90€ Rabatt nötig
        // Haftpflicht 100€ mit 99% = 99€ Rabatt -> zu viel!
        // Optimal: Haftpflicht ~82% (82€) + Kasko ~16% (8€) = 90€ Rabatt
        // Oder ähnliche Kombination, die 60€ Netto ergibt
        
        // Haftpflicht sollte sehr stark rabattiert sein (da teurer)
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isGreaterThan(70);
        
        // Die Nettosumme muss stimmen
        double nettoGesamt = tarifierung.getHaftpflicht().getPraemieMitRabatt() 
            + tarifierung.getKasko().getPraemieMitRabatt();
        assertThat(nettoGesamt).isCloseTo(60, within(1.0));
    }

    @Test
    @DisplayName("Zielbeitrag erfordert beide Rabatte (Kasko teurer)")
    void zielbeitrag_beideRabatte_kaskoTeurer() {
        var haftpflicht = new Haftpflicht(50);
        var kasko = new Kasko(100);
        var tarifierung = new Tarifierung(60, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(60, within(1.0));
    
        // Beide Produkte sollten hohen Rabatt haben
        // Kasko (teurer) wird zuerst rabattiert, aber 99% wäre zu viel
        // Mathematik: 90€ Rabatt nötig
        // Optimal: Kasko ~89-91%, Haftpflicht minimal oder umgekehrt optimiert
        assertThat(tarifierung.getKasko().getRabatt()).isGreaterThan(80);
        
        // Die Nettosumme muss stimmen
        double nettoGesamt = tarifierung.getHaftpflicht().getPraemieMitRabatt() 
            + tarifierung.getKasko().getPraemieMitRabatt();
        assertThat(nettoGesamt).isCloseTo(60, within(1.0));
    }

    @Test
    @DisplayName("Zielbeitrag gleich Gesamtprämie - kein Rabatt nötig")
    void zielbeitrag_gleichGesamtpraemie() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(150, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(150);
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isEqualTo(0);
        assertThat(tarifierung.getKasko().getRabatt()).isEqualTo(0);
    }

    @Test
    @DisplayName("Zielbeitrag höher als Gesamtprämie - kein Rabatt nötig")
    void zielbeitrag_hoeherAlsGesamtpraemie() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(200, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(150);
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isEqualTo(0);
        assertThat(tarifierung.getKasko().getRabatt()).isEqualTo(0);
    }

    @Test
    @DisplayName("Zielbeitrag nur mit Haftpflicht (ohne Kasko)")
    void zielbeitrag_nurHaftpflicht() {
        var haftpflicht = new Haftpflicht(100);
        var tarifierung = new Tarifierung(80, haftpflicht, null);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(100);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(80, within(0.5));
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isBetween(15, 25);
    }

    @Test
    @DisplayName("Zielbeitrag nicht erreichbar - Exception")
    void zielbeitrag_nichtErreichbar() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);

        // Minimaler Beitrag bei 99% Rabatt: 150 * 0.01 = 1.5
        assertThatThrownBy(() -> new Tarifierung(1.0, haftpflicht, kasko))
            .isInstanceOf(PlausiException.class)
            .hasMessageContaining("nicht erreichbar")
            .hasMessageContaining("Minimaler Beitrag");
    }

    @Test
    @DisplayName("Sehr niedriger Zielbeitrag (aber erreichbar)")
    void zielbeitrag_sehrNiedrig() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);
        var tarifierung = new Tarifierung(2.0, haftpflicht, kasko);

        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isCloseTo(2.0, within(0.5));
    
        // Beide sollten nahe am maximalen Rabatt sein
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isGreaterThanOrEqualTo(98);
        assertThat(tarifierung.getKasko().getRabatt()).isGreaterThanOrEqualTo(98);
    }

    @Test
    @DisplayName("Optimierung minimiert Prozent-Rabatt (Haftpflicht teurer)")
    void zielbeitrag_minimalerProzentRabatt_haftpflichtTeurer() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(20);
        var tarifierung = new Tarifierung(110, haftpflicht, kasko);

        // 10€ Rabatt nötig: Haftpflicht mit 10% = besser als Kasko mit 50%
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isGreaterThan(0);
        assertThat(tarifierung.getKasko().getRabatt()).isLessThanOrEqualTo(5);
        
        int gesamtRabattProzent = tarifierung.getHaftpflicht().getRabatt() 
            + tarifierung.getKasko().getRabatt();
        assertThat(gesamtRabattProzent).isLessThan(20);
    }

    @Test
    @DisplayName("Optimierung minimiert Prozent-Rabatt (Kasko teurer)")
    void zielbeitrag_minimalerProzentRabatt_kaskoTeurer() {
        var haftpflicht = new Haftpflicht(20);
        var kasko = new Kasko(100);
        var tarifierung = new Tarifierung(110, haftpflicht, kasko);

        // 10€ Rabatt nötig: Kasko mit 10% = besser als Haftpflicht mit 50%
        assertThat(tarifierung.getKasko().getRabatt()).isGreaterThan(0);
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isLessThanOrEqualTo(5);
        
        int gesamtRabattProzent = tarifierung.getHaftpflicht().getRabatt() 
            + tarifierung.getKasko().getRabatt();
        assertThat(gesamtRabattProzent).isLessThan(20);
    }
}
