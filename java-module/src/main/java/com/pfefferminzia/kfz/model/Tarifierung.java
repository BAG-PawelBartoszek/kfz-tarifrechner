package com.pfefferminzia.kfz.model;

import com.pfefferminzia.kfz.service.ValidationService;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;

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

    public void berechneTarifierung() {
        ValidationService.validate(this); // Einfacher Aufruf!
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
        //ToDo: Implementieren
    }
}