package com.pfefferminzia.kfz.model;

import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.Positive;

public abstract class Produkt {
    
    @Positive(message = "Prämie muss positiv sein")
    double praemie;
    
    @Min(value = 0, message = "Rabatt darf nicht negativ sein")
    @Max(value = 99, message = "Rabatt darf nicht größer 99 sein")
    int rabatt;

    public Produkt(double praemie, int rabatt) {
        this.praemie = praemie;
        this.rabatt = rabatt;
    }

    public Produkt(double praemie) {
        this.praemie = praemie;
        this.rabatt = 0;
    }

    public double getPraemie() {
        return praemie;
    }

    public int getRabatt() {
        return rabatt;
    }

    public void setRabatt(int rabatt) {
        this.rabatt = rabatt;
    }

    public double getPraemieMitRabatt() {
        return praemie * ((100 - rabatt) / 100.0);
    }
}
