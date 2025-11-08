"""Abstrakte Basisklasse für Versicherungsprodukte"""
from abc import ABC


class Produkt(ABC):
    """Abstrakte Basisklasse für Haftpflicht und Kasko"""

    def __init__(self, praemie: float, rabatt: int = 0):
        """
        Initialisiert ein Produkt

        Args:
            praemie: Prämie (muss positiv sein)
            rabatt: Rabatt in Prozent (0-99)
        """
        self._praemie = praemie
        self._rabatt = rabatt

    @property
    def praemie(self) -> float:
        """Gibt die Prämie zurück"""
        return self._praemie

    @property
    def rabatt(self) -> int:
        """Gibt den Rabatt zurück"""
        return self._rabatt

    @rabatt.setter
    def rabatt(self, value: int):
        """Setzt den Rabatt"""
        self._rabatt = value

    def get_praemie_mit_rabatt(self) -> float:
        """Berechnet die Prämie nach Anwendung des Rabatts"""
        return self._praemie * ((100 - self._rabatt) / 100.0)