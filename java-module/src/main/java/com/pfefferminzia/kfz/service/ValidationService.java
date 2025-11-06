package com.pfefferminzia.kfz.service;

import com.pfefferminzia.kfz.common.PlausiException;
import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validation;
import jakarta.validation.Validator;
import jakarta.validation.ValidatorFactory;

import java.util.List;
import java.util.Set;

public class ValidationService {
    
    private static final Validator validator;
    
    static {
        try (ValidatorFactory factory = Validation.buildDefaultValidatorFactory()) {
            validator = factory.getValidator();
        }
    }

    public static <T> void validate(T object) {
        Set<ConstraintViolation<T>> violations = validator.validate(object);
        
        if (!violations.isEmpty()) {
            List<String> fehler = violations.stream()
                .map(ValidationService::formatFehler)
                .toList();
            
            throw new PlausiException(fehler);
        }
    }
    
    private static <T> String formatFehler(ConstraintViolation<T> violation) {
        String propertyPath = violation.getPropertyPath().toString();
        String message = violation.getMessage();
        
        // Formatierung für verschachtelte Objekte
        if (propertyPath.contains(".")) {
            String[] parts = propertyPath.split("\\.");
            String objectName = parts[0];
            String fieldName = parts[1];
            
            return switch (objectName) {
                case "haftpflicht" -> formatHaftpflichtFehler(fieldName, message);
                case "kasko" -> formatKaskoFehler(fieldName, message);
                default -> message;
            };
        }
        
        return message;
    }
    
    private static String formatHaftpflichtFehler(String field, String message) {
        return switch (field) {
            case "praemie" -> "Prämie für die Haftpflicht muss positiv sein";
            case "rabatt" -> {
                if (message.contains("negativ")) {
                    yield "Rabatt für die Haftpflicht darf nicht negativ sein";
                } else {
                    yield "Rabatt für die Haftpflicht darf nicht größer 99 sein";
                }
            }
            default -> message;
        };
    }
    
    private static String formatKaskoFehler(String field, String message) {
        return switch (field) {
            case "praemie" -> "Prämie für die Kasko muss positiv sein";
            case "rabatt" -> {
                if (message.contains("negativ")) {
                    yield "Rabatt für die Kasko darf nicht negativ sein";
                } else {
                    yield "Rabatt für die Kasko darf nicht größer 99 sein";
                }
            }
            default -> message;
        };
    }
}
