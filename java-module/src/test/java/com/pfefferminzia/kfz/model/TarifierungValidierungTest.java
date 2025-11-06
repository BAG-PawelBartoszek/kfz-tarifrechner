package com.pfefferminzia.kfz.model;

import com.pfefferminzia.kfz.common.PlausiException;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

@DisplayName("Tarifierung Validierung Tests")
class TarifierungValidierungTest {

    @Test
    @DisplayName("Haftpflicht null - sollte PlausiException werfen")
    void validierung_haftpflichtNull_wirftException() {
        assertThatThrownBy(() -> new Tarifierung(null))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Haftpflicht ist Pflichtfeld");
    }

    @Test
    @DisplayName("Haftpflicht Prämie negativ - sollte PlausiException werfen")
    void validierung_haftpflichtPraemieNegativ_wirftException() {
        var haftpflicht = new Haftpflicht(-100);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Prämie für die Haftpflicht muss positiv sein");
    }

    @Test
    @DisplayName("Haftpflicht Prämie null (0) - sollte PlausiException werfen")
    void validierung_haftpflichtPraemieNull_wirftException() {
        var haftpflicht = new Haftpflicht(0);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Prämie für die Haftpflicht muss positiv sein");
    }

    @Test
    @DisplayName("Haftpflicht Rabatt negativ - sollte PlausiException werfen")
    void validierung_haftpflichtRabattNegativ_wirftException() {
        var haftpflicht = new Haftpflicht(100, -10);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Rabatt für die Haftpflicht darf nicht negativ sein");
    }

    @Test
    @DisplayName("Haftpflicht Rabatt >= 100 - sollte PlausiException werfen")
    void validierung_haftpflichtRabatt100_wirftException() {
        var haftpflicht = new Haftpflicht(100, 100);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Rabatt für die Haftpflicht darf nicht größer 99 sein");
    }

    @Test
    @DisplayName("Haftpflicht Rabatt > 100 - sollte PlausiException werfen")
    void validierung_haftpflichtRabattUeber100_wirftException() {
        var haftpflicht = new Haftpflicht(100, 150);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Rabatt für die Haftpflicht darf nicht größer 99 sein");
    }

    @Test
    @DisplayName("Kasko Prämie negativ - sollte PlausiException werfen")
    void validierung_kaskoPraemieNegativ_wirftException() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(-50);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Prämie für die Kasko muss positiv sein");
    }

    @Test
    @DisplayName("Kasko Prämie null (0) - sollte PlausiException werfen")
    void validierung_kaskoPraemieNull_wirftException() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(0);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Prämie für die Kasko muss positiv sein");
    }

    @Test
    @DisplayName("Kasko Rabatt negativ - sollte PlausiException werfen")
    void validierung_kaskoRabattNegativ_wirftException() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50, -10);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Rabatt für die Kasko darf nicht negativ sein");
    }

    @Test
    @DisplayName("Kasko Rabatt >= 100 - sollte PlausiException werfen")
    void validierung_kaskoRabatt100_wirftException() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50, 100);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Rabatt für die Kasko darf nicht größer 99 sein");
    }

    @Test
    @DisplayName("Zielbeitrag negativ - sollte PlausiException werfen")
    void validierung_zielbeitragNegativ_wirftException() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);

        assertThatThrownBy(() -> new Tarifierung(-200, haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Zielbeitrag von -200,00€ ist nicht erreichbar");
    }

    @Test
    @DisplayName("Zielbeitrag null (0) - sollte PlausiException werfen")
    void validierung_zielbeitragNull_wirftException() {
        var haftpflicht = new Haftpflicht(100);
        var kasko = new Kasko(50);

        assertThatThrownBy(() -> new Tarifierung(0, haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .hasMessageContaining("Zielbeitrag von 0,00€ ist nicht erreichbar");
    }

    @Test
    @DisplayName("Multiple Fehler - sollte alle Fehler in PlausiException sammeln")
    void validierung_multipleFehler_sammeltAlleFehler() {
        var haftpflicht = new Haftpflicht(-100, -10);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht))
                .isInstanceOf(PlausiException.class)
                .satisfies(exception -> {
                    PlausiException plausiException = (PlausiException) exception;
                    assertThat(plausiException.getFehler()).hasSize(2);
                    assertThat(plausiException.getFehler())
                            .contains("Prämie für die Haftpflicht muss positiv sein")
                            .contains("Rabatt für die Haftpflicht darf nicht negativ sein");
                });
    }

    @Test
    @DisplayName("Haftpflicht und Kasko mit mehreren Fehlern")
    void validierung_haftpflichtUndKaskoMultipleFehler_sammeltAlleFehler() {
        var haftpflicht = new Haftpflicht(-100, 150);
        var kasko = new Kasko(-50, -20);

        assertThatThrownBy(() -> new Tarifierung(haftpflicht, kasko))
                .isInstanceOf(PlausiException.class)
                .satisfies(exception -> {
                    PlausiException plausiException = (PlausiException) exception;
                    assertThat(plausiException.getFehler()).hasSize(4);
                    assertThat(plausiException.getFehler())
                            .contains("Prämie für die Haftpflicht muss positiv sein")
                            .contains("Rabatt für die Haftpflicht darf nicht größer 99 sein")
                            .contains("Prämie für die Kasko muss positiv sein")
                            .contains("Rabatt für die Kasko darf nicht negativ sein");
                });
    }

    @Test
    @DisplayName("Gültige Haftpflicht ohne Kasko - keine Exception")
    void validierung_gueltigeHaftpflichtOhneKasko_keineException() {
        var haftpflicht = new Haftpflicht(100, 10);

        var tarifierung = new Tarifierung(haftpflicht);

        assertThat(tarifierung).isNotNull();
        assertThat(tarifierung.getHaftpflicht()).isEqualTo(haftpflicht);
        assertThat(tarifierung.getKasko()).isNull();
    }

    @Test
    @DisplayName("Gültige Haftpflicht mit Kasko - keine Exception")
    void validierung_gueltigeHaftpflichtMitKasko_keineException() {
        var haftpflicht = new Haftpflicht(100, 10);
        var kasko = new Kasko(50, 15);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung).isNotNull();
        assertThat(tarifierung.getHaftpflicht()).isEqualTo(haftpflicht);
        assertThat(tarifierung.getKasko()).isEqualTo(kasko);
    }

    @Test
    @DisplayName("Gültiger Zielbeitrag - keine Exception")
    void validierung_gueltigerZielbeitrag_keineException() {
        var haftpflicht = new Haftpflicht(100, 10);
        var kasko = new Kasko(50, 15);

        var tarifierung = new Tarifierung(200, haftpflicht, kasko);

        assertThat(tarifierung).isNotNull();
        assertThat(tarifierung.getZielbeitrag()).isEqualTo(200);
    }

    @Test
    @DisplayName("Rabatt 0 ist gültig")
    void validierung_rabattNull_istGueltig() {
        var haftpflicht = new Haftpflicht(100, 0);
        var kasko = new Kasko(50, 0);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung).isNotNull();
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isEqualTo(0);
        assertThat(tarifierung.getKasko().getRabatt()).isEqualTo(0);
    }

    @Test
    @DisplayName("Rabatt 99 ist gültig (Grenzwert)")
    void validierung_rabatt99_istGueltig() {
        var haftpflicht = new Haftpflicht(100, 99);
        var kasko = new Kasko(50, 99);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung).isNotNull();
        assertThat(tarifierung.getHaftpflicht().getRabatt()).isEqualTo(99);
        assertThat(tarifierung.getKasko().getRabatt()).isEqualTo(99);
    }

    @Test
    @DisplayName("Sehr kleine Prämie (0.01) ist gültig")
    void validierung_sehrKleinePraemie_istGueltig() {
        var haftpflicht = new Haftpflicht(0.01, 0);
        var kasko = new Kasko(0.01, 0);

        var tarifierung = new Tarifierung(haftpflicht, kasko);

        assertThat(tarifierung).isNotNull();
        assertThat(tarifierung.getHaftpflicht().getPraemie()).isEqualTo(0.01);
        assertThat(tarifierung.getKasko().getPraemie()).isEqualTo(0.01);
    }
}