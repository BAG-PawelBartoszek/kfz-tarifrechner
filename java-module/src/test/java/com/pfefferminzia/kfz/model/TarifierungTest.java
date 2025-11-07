package com.pfefferminzia.kfz.model;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.assertj.core.api.Assertions.assertThat;

@DisplayName("Tarifierung Tests")
class TarifierungTest {

    @Test
    @DisplayName("Berechnung ohne Zielbeitrag, ohne Rabatt und ohne Kasko")
    void berechneTarifierung_keinZielbeitrag_ohneKasko_keinRabatt() {
        var haftpflicht = new Haftpflicht(75);
        var tarifierung = new Tarifierung(haftpflicht);

        assertThat(tarifierung.getZielbeitrag()).isNull();
        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(75);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(75);
    }

    @Test
    @DisplayName("Berechnung ohne Zielbeitrag, ohne Rabatt und mit Kasko")
    void berechneTarifierung_keinZielbeitrag_mitKasko_keineRabatte() {
        var kasko = new Kasko(50);
        var haftpflicht = new Haftpflicht(75);
        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung.getZielbeitrag()).isNull();
        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(125);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(125);
    }

    @Test
    @DisplayName("Berechnung mit Haftpflicht-Rabatt")
    void berechneTarifierung_mitHaftpflichtRabatt() {
        var haftpflicht = new Haftpflicht(100, 20); // 20% Rabatt
        var tarifierung = new Tarifierung(haftpflicht);

        assertThat(tarifierung.getZielbeitrag()).isNull();
        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(100);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(80); // 100 - 20%
    }

    @Test
    @DisplayName("Berechnung mit Kasko-Rabatt")
    void berechneTarifierung_mitKaskoRabatt() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50, 10); // 10% Rabatt
        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung.getZielbeitrag()).isNull();
        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(145); // 100 + (50 - 5)
    }

    @Test
    @DisplayName("Berechnung mit beiden Rabatten")
    void berechneTarifierung_mitHaftpflichtUndKaskoRabatt() {
        var haftpflicht = new Haftpflicht(100, 20); // 20% Rabatt
        var kasko = new Kasko(50, 10); // 10% Rabatt
        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung.getZielbeitrag()).isNull();
        assertThat(tarifierung.getGesamtpraemie()).isEqualTo(150);
        assertThat(tarifierung.getNettopraemie()).isEqualTo(125); // 80 + 45
    }
}