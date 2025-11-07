package com.pfefferminzia.kfz.model;

import com.pfefferminzia.kfz.common.PlausiException;
import com.pfefferminzia.kfz.service.ValidationService;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;

import java.util.List;

public class Tarifierung {

    @Positive(message = "Zielbeitrag muss positiv sein")
    Double zielbeitrag;

    double gesamtpraemie;
    double nettopraemie;

    @NotNull(message = "Haftpflicht ist Pflichtfeld")
    @Valid
    Haftpflicht haftpflicht;

    @Valid
    Kasko kasko;

    public Tarifierung(Haftpflicht haftpflicht) {
        this.haftpflicht = haftpflicht;
        berechneTarifierung();
    }

    public Tarifierung(Haftpflicht haftpflicht, Kasko kasko) {
        this.haftpflicht = haftpflicht;
        this.kasko = kasko;
        berechneTarifierung();
    }

    public Tarifierung(double zielbeitrag, Haftpflicht haftpflicht, Kasko kasko) {
        this.haftpflicht = haftpflicht;
        this.kasko = kasko;
        this.zielbeitrag = zielbeitrag;
        berechneRabatteFuerZielbeitrag();
        berechneTarifierung();
    }

    public Double getZielbeitrag() {
        return zielbeitrag;
    }

    public double getGesamtpraemie() {
        return gesamtpraemie;
    }

    public double getNettopraemie() {
        return nettopraemie;
    }

    public Haftpflicht getHaftpflicht() {
        return haftpflicht;
    }

    public Kasko getKasko() {
        return kasko;
    }

    private void berechneTarifierung() {
        ValidationService.validate(this);
        this.gesamtpraemie = berechneGesamtpraemie();
        this.nettopraemie = berechneNettopraemie();
    }

    private double berechneNettopraemie() {
        var nettoHaftpflicht = this.haftpflicht.getPraemieMitRabatt();
        var nettoKasko = java.util.Optional.ofNullable(this.kasko)
            .map(Produkt::getPraemieMitRabatt)
            .orElse(0.0);

        return nettoHaftpflicht + nettoKasko;
    }

    private double berechneGesamtpraemie() {
        double haftpflichtPraemie = this.haftpflicht.getPraemie();
        double kaskoPraemie = java.util.Optional.ofNullable(this.kasko)
            .map(Kasko::getPraemie)
            .orElse(0.0);

        return haftpflichtPraemie + kaskoPraemie;
    }

    private void berechneRabatteFuerZielbeitrag() {
        if (zielbeitrag == null) {
            return;
        }

        double aktuelleGesamtpraemie = this.haftpflicht.getPraemie() 
            + (this.kasko != null ? this.kasko.getPraemie() : 0.0);

        // Prüfen, ob Zielbeitrag überhaupt erreichbar ist
        if (zielbeitrag >= aktuelleGesamtpraemie) {
            // Kein Rabatt nötig, Zielbeitrag ist höher als Gesamtprämie
            return;
        }

        // Minimaler erreichbarer Beitrag bei maximalem Rabatt (99%)
        double minimalerBeitrag = aktuelleGesamtpraemie * 0.01; // 1% der Gesamtprämie
        
        if (zielbeitrag < minimalerBeitrag) {
            throw new PlausiException(List.of(
                String.format("Zielbeitrag von %.2f€ ist nicht erreichbar. " +
                    "Minimaler Beitrag bei maximalem Rabatt (99%%): %.2f€",
                    zielbeitrag, minimalerBeitrag)
            ));
        }

        // Strategie: Das TEURERE Produkt zuerst rabattieren
        // Dies minimiert die Rabatt-Prozente (weniger % für gleichen Betrag)
        
        double benoetigterRabatt = aktuelleGesamtpraemie - zielbeitrag;
        
        if (this.kasko != null) {
            // Bestimme, welches Produkt teurer ist
            boolean kaskoIstTeurer = this.kasko.getPraemie() > this.haftpflicht.getPraemie();
            
            if (kaskoIstTeurer) {
                verteileRabatt(benoetigterRabatt, this.kasko, this.haftpflicht);
            } else {
                verteileRabatt(benoetigterRabatt, this.haftpflicht, this.kasko);
            }
        } else {
            // Nur Haftpflicht vorhanden, gesamter Rabatt auf Haftpflicht
            int haftpflichtRabattProzent = berechneRabattProzent(benoetigterRabatt, this.haftpflicht.getPraemie());
            this.haftpflicht.setRabatt(haftpflichtRabattProzent);
        }

        // Feinabstimmung: Iterativ optimieren, um genau den Zielbeitrag zu treffen
        optimiereRabatte();
    }

    /**
     * Berechnet den benötigten Rabatt in Prozent und begrenzt ihn auf maximal 99%
     */
    private int berechneRabattProzent(double benoetigterBetrag, double praemie) {
        if (praemie == 0) return 0;
        int prozent = (int) Math.ceil((benoetigterBetrag / praemie) * 100);
        return Math.min(prozent, 99);
    }

    /**
     * Verteilt den benötigten Rabatt, indem zuerst das teurere Produkt rabattiert wird.
     * Dies minimiert die Anzahl der Rabatt-Prozente.
     */
    private void verteileRabatt(double benoetigterRabatt, Produkt teureres, Produkt guenstigeres) {
        double maxTeureresRabatt = teureres.getPraemie() * 0.99; // 99% Rabatt möglich
    
        if (benoetigterRabatt <= maxTeureresRabatt) {
            // Rabatt kann komplett über das teurere Produkt erreicht werden
            int rabattProzent = berechneRabattProzent(benoetigterRabatt, teureres.getPraemie());
            teureres.setRabatt(rabattProzent);
            guenstigeres.setRabatt(0); // Explizit auf 0 setzen
        } else {
            // Maximaler Rabatt auf teureres, Rest auf günstigeres
            teureres.setRabatt(99);
            double verbleibendenRabatt = benoetigterRabatt - maxTeureresRabatt;
            int guenstigeresProzent = berechneRabattProzent(verbleibendenRabatt, guenstigeres.getPraemie());
            guenstigeres.setRabatt(guenstigeresProzent);
        }
    }

    private void optimiereRabatte() {
        final double TOLERANZ = 0.01; // 1 Cent Toleranz
        final int MAX_ITERATIONEN = 100;
        
        for (int i = 0; i < MAX_ITERATIONEN; i++) {
            double aktuellerNettobeitrag = berechneAktuellenNettobeitrag();
            double differenz = aktuellerNettobeitrag - zielbeitrag;
            
            // Ziel erreicht (mit Toleranz)
            if (Math.abs(differenz) < TOLERANZ) {
                break;
            }
            
            // Zu teuer - mehr Rabatt nötig
            if (differenz > 0) {
                if (!erhoeheRabatt()) {
                    break; // Maximaler Rabatt erreicht
                }
            }
            // Zu günstig - Rabatt reduzieren
            else {
                if (!reduziereRabatt()) {
                    break; // Minimaler Rabatt erreicht
                }
            }
        }
    }

    private double berechneAktuellenNettobeitrag() {
        double nettoHaftpflicht = this.haftpflicht.getPraemieMitRabatt();
        double nettoKasko = this.kasko != null ? this.kasko.getPraemieMitRabatt() : 0.0;
        return nettoHaftpflicht + nettoKasko;
    }

    private boolean erhoeheRabatt() {
        // Priorität: Das TEURERE Produkt zuerst (minimiert Prozent-Rabatt)
        if (this.kasko != null) {
            boolean kaskoIstTeurer = this.kasko.getPraemie() > this.haftpflicht.getPraemie();
        
            if (kaskoIstTeurer) {
                if (this.kasko.getRabatt() < 99) {
                    this.kasko.setRabatt(Math.min(this.kasko.getRabatt() + 1, 99));
                    return true;
                }
                if (this.haftpflicht.getRabatt() < 99) {
                    this.haftpflicht.setRabatt(Math.min(this.haftpflicht.getRabatt() + 1, 99));
                    return true;
                }
            } else {
                if (this.haftpflicht.getRabatt() < 99) {
                    this.haftpflicht.setRabatt(Math.min(this.haftpflicht.getRabatt() + 1, 99));
                    return true;
                }
                if (this.kasko.getRabatt() < 99) {
                    this.kasko.setRabatt(Math.min(this.kasko.getRabatt() + 1, 99));
                    return true;
                }
            }
        } else {
            if (this.haftpflicht.getRabatt() < 99) {
                this.haftpflicht.setRabatt(Math.min(this.haftpflicht.getRabatt() + 1, 99));
                return true;
            }
        }
        return false;
    }

    private boolean reduziereRabatt() {
        // Priorität: Das GÜNSTIGERE Produkt zuerst reduzieren (umgekehrte Reihenfolge)
        if (this.kasko != null) {
            boolean kaskoIstTeurer = this.kasko.getPraemie() > this.haftpflicht.getPraemie();
        
            if (kaskoIstTeurer) {
                // Haftpflicht ist günstiger -> erst Haftpflicht reduzieren
                if (this.haftpflicht.getRabatt() > 0) {
                    this.haftpflicht.setRabatt(Math.max(this.haftpflicht.getRabatt() - 1, 0));
                    return true;
                }
                if (this.kasko.getRabatt() > 0) {
                    this.kasko.setRabatt(Math.max(this.kasko.getRabatt() - 1, 0));
                    return true;
                }
            } else {
                // Kasko ist günstiger oder gleich -> erst Kasko reduzieren
                if (this.kasko.getRabatt() > 0) {
                    this.kasko.setRabatt(Math.max(this.kasko.getRabatt() - 1, 0));
                    return true;
                }
                if (this.haftpflicht.getRabatt() > 0) {
                    this.haftpflicht.setRabatt(Math.max(this.haftpflicht.getRabatt() - 1, 0));
                    return true;
                }
            }
        } else {
            if (this.haftpflicht.getRabatt() > 0) {
                this.haftpflicht.setRabatt(Math.max(this.haftpflicht.getRabatt() - 1, 0));
                return true;
            }
        }
        return false;
    }
}