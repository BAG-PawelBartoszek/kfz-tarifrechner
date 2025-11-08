"""Validierungsservice für Tarifierung und Produkte"""
from typing import List, Optional
from ..common.plausi_exception import PlausiException


class ValidationService:
    """Service zur Validierung von Tarifierungs-Objekten"""

    @staticmethod
    def validate(obj):
        """Validiert ein Objekt"""
        from ..model.tarifierung import Tarifierung
        from ..model.produkt import Produkt

        fehler: List[str] = []

        if isinstance(obj, Tarifierung):
            fehler.extend(ValidationService._validate_tarifierung(obj))
        elif isinstance(obj, Produkt):
            fehler.extend(ValidationService._validate_produkt(obj, None))

        if fehler:
            raise PlausiException(fehler)

    @staticmethod
    def _validate_tarifierung(tarifierung) -> List[str]:
        """Validiert eine Tarifierung"""
        fehler: List[str] = []

        # Haftpflicht ist Pflichtfeld
        if tarifierung.haftpflicht is None:
            fehler.append("Haftpflicht ist Pflichtfeld")
        else:
            fehler.extend(
                ValidationService._validate_produkt(tarifierung.haftpflicht, "haftpflicht")
            )

        # Kasko validieren (falls vorhanden)
        if tarifierung.kasko is not None:
            fehler.extend(
                ValidationService._validate_produkt(tarifierung.kasko, "kasko")
            )

        # Zielbeitrag validieren (falls vorhanden)
        if tarifierung.zielbeitrag is not None and tarifierung.zielbeitrag <= 0:
            # Berechne minimalen Beitrag für bessere Fehlermeldung
            aktuelle_gesamtpraemie = (
                    tarifierung.haftpflicht.praemie +
                    (tarifierung.kasko.praemie if tarifierung.kasko else 0.0)
            )
            minimaler_beitrag = aktuelle_gesamtpraemie * 0.01

            fehler.append(
                f"Zielbeitrag von {tarifierung.zielbeitrag:.2f}€ ist nicht erreichbar. "
                f"Minimaler Beitrag bei maximalem Rabatt (99%): {minimaler_beitrag:.2f}€"
            )

        return fehler

    @staticmethod
    def _validate_produkt(produkt, kontext: Optional[str]) -> List[str]:
        """Validiert ein Produkt"""
        fehler: List[str] = []

        # Prämie muss positiv sein
        if produkt.praemie <= 0:
            if kontext == "haftpflicht":
                fehler.append("Prämie für die Haftpflicht muss positiv sein")
            elif kontext == "kasko":
                fehler.append("Prämie für die Kasko muss positiv sein")
            else:
                fehler.append("Prämie muss positiv sein")

        # Rabatt muss zwischen 0 und 99 liegen
        if produkt.rabatt < 0:
            if kontext == "haftpflicht":
                fehler.append("Rabatt für die Haftpflicht darf nicht negativ sein")
            elif kontext == "kasko":
                fehler.append("Rabatt für die Kasko darf nicht negativ sein")
            else:
                fehler.append("Rabatt darf nicht negativ sein")
        elif produkt.rabatt > 99:
            if kontext == "haftpflicht":
                fehler.append("Rabatt für die Haftpflicht darf nicht größer 99 sein")
            elif kontext == "kasko":
                fehler.append("Rabatt für die Kasko darf nicht größer 99 sein")
            else:
                fehler.append("Rabatt darf nicht größer 99 sein")

        return fehler