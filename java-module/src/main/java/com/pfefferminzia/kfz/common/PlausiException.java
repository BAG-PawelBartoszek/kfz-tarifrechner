package com.pfefferminzia.kfz.common;

import java.util.List;

public class PlausiException extends RuntimeException {

    private final List<String> fehler;

    public PlausiException(List<String> fehler) {
        super(erstelleFehlermeldung(fehler));
        this.fehler = fehler;
        // Ausgabe in Console
        System.err.println(getMessage());
    }

    private static String erstelleFehlermeldung(List<String> fehler) {
        if (fehler == null || fehler.isEmpty()) {
            return "Plausibilitätsprüfung fehlgeschlagen";
        }

        StringBuilder sb = new StringBuilder();
        sb.append("\n");
        sb.append("=".repeat(70)).append("\n");
        sb.append("PLAUSIBILITÄTSFEHLER\n");
        sb.append("=".repeat(70)).append("\n");
        
        for (int i = 0; i < fehler.size(); i++) {
            sb.append(String.format("  %d. %s\n", i + 1, fehler.get(i)));
        }
        
        sb.append("=".repeat(70));
        
        return sb.toString();
    }

    public List<String> getFehler() {
        return fehler;
    }
}
