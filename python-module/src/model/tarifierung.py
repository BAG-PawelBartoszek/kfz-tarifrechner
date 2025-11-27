"""Tarifierung für KFZ-Versicherung"""
from typing import Optional
from .haftpflicht import Haftpflicht
from .kasko import Kasko
from ..service.validation_service import ValidationService
from ..common.plausi_exception import PlausiException


class Tarifierung:
    """Tarifierung für KFZ-Versicherung mit Haftpflicht und optionaler Kasko"""

    def __init__(
            self,
            zielbeitrag: Optional[float],
            haftpflicht: Haftpflicht,
            kasko: Optional[Kasko] = None
    ):
        """
        Konstruktor mit Zielbeitrag

        Args:
            zielbeitrag: Gewünschter Zielbeitrag (None wenn kein Ziel)
            haftpflicht: Haftpflicht-Produkt (Pflichtfeld)
            kasko: Kasko-Produkt (optional)
        """
        self._zielbeitrag = zielbeitrag
        self._haftpflicht = haftpflicht
        self._kasko = kasko
        self._gesamtpraemie = 0.0
        self._nettopraemie = 0.0

        # Zuerst Basis-Validierung durchführen
        ValidationService.validate(self)

        if zielbeitrag is not None:
            self._berechne_rabatte_fuer_zielbeitrag()

        self._berechne_tarifierung()

    @classmethod
    def ohne_zielbeitrag(
            cls,
            haftpflicht: Haftpflicht,
            kasko: Optional[Kasko] = None
    ) -> 'Tarifierung':
        """Factory-Methode für Tarifierung ohne Zielbeitrag"""
        return cls(None, haftpflicht, kasko)

    @property
    def zielbeitrag(self) -> Optional[float]:
        """Gibt den Zielbeitrag zurück"""
        return self._zielbeitrag

    @property
    def gesamtpraemie(self) -> float:
        """Gibt die Gesamtprämie zurück"""
        return self._gesamtpraemie

    @property
    def nettopraemie(self) -> float:
        """Gibt die Nettoprämie zurück"""
        return self._nettopraemie

    @property
    def haftpflicht(self) -> Haftpflicht:
        """Gibt die Haftpflicht zurück"""
        return self._haftpflicht

    @property
    def kasko(self) -> Optional[Kasko]:
        """Gibt die Kasko zurück"""
        return self._kasko

    def _berechne_tarifierung(self):
        """Führt die Tarifierung durch"""
        self._gesamtpraemie = self._berechne_gesamtpraemie()
        self._nettopraemie = self._berechne_nettopraemie()

    def _berechne_nettopraemie(self) -> float:
        """Berechnet die Nettoprämie"""
        netto_haftpflicht = self._haftpflicht.get_praemie_mit_rabatt()
        netto_kasko = self._kasko.get_praemie_mit_rabatt() if self._kasko else 0.0
        return netto_haftpflicht + netto_kasko

    def _berechne_gesamtpraemie(self) -> float:
        """Berechnet die Gesamtprämie"""
        haftpflicht_praemie = self._haftpflicht.praemie
        kasko_praemie = self._kasko.praemie if self._kasko else 0.0
        return haftpflicht_praemie + kasko_praemie

    def _berechne_rabatte_fuer_zielbeitrag(self):
        """ToDo: Implementiere mich!!"""