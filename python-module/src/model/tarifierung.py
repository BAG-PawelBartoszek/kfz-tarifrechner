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
        """Berechnet die Rabatte für den gewünschten Zielbeitrag"""
        if self._zielbeitrag is None:
            return

        aktuelle_gesamtpraemie = (
                self._haftpflicht.praemie +
                (self._kasko.praemie if self._kasko else 0.0)
        )

        # Prüfen, ob Zielbeitrag überhaupt erreichbar ist
        if self._zielbeitrag >= aktuelle_gesamtpraemie:
            # Kein Rabatt nötig
            return

        # Minimaler erreichbarer Beitrag bei maximalem Rabatt (99%)
        minimaler_beitrag = aktuelle_gesamtpraemie * 0.01

        if self._zielbeitrag < minimaler_beitrag:
            raise PlausiException([
                f"Zielbeitrag von {self._zielbeitrag:.2f}€ ist nicht erreichbar. "
                f"Minimaler Beitrag bei maximalem Rabatt (99%): {minimaler_beitrag:.2f}€"
            ])

        # Strategie: Das TEURERE Produkt zuerst rabattieren
        benoetigter_rabatt = aktuelle_gesamtpraemie - self._zielbeitrag

        if self._kasko:
            # Bestimme, welches Produkt teurer ist
            kasko_ist_teurer = self._kasko.praemie > self._haftpflicht.praemie

            if kasko_ist_teurer:
                self._verteile_rabatt(benoetigter_rabatt, self._kasko, self._haftpflicht)
            else:
                self._verteile_rabatt(benoetigter_rabatt, self._haftpflicht, self._kasko)
        else:
            # Nur Haftpflicht vorhanden
            haftpflicht_rabatt_prozent = self._berechne_rabatt_prozent(
                benoetigter_rabatt,
                self._haftpflicht.praemie
            )
            self._haftpflicht.rabatt = haftpflicht_rabatt_prozent

        # Feinabstimmung
        self._optimiere_rabatte()

    def _berechne_rabatt_prozent(self, benoetigter_betrag: float, praemie: float) -> int:
        """Berechnet den Rabatt in Prozent"""
        if praemie == 0:
            return 0
        import math
        prozent = math.ceil((benoetigter_betrag / praemie) * 100)
        return min(prozent, 99)

    def _verteile_rabatt(self, benoetigter_rabatt: float, teureres, guenstigeres):
        """Verteilt den Rabatt zwischen zwei Produkten"""
        max_teureres_rabatt = teureres.praemie * 0.99

        if benoetigter_rabatt <= max_teureres_rabatt:
            # Rabatt kann komplett über das teurere Produkt erreicht werden
            rabatt_prozent = self._berechne_rabatt_prozent(benoetigter_rabatt, teureres.praemie)
            teureres.rabatt = rabatt_prozent
            guenstigeres.rabatt = 0
        else:
            # Maximaler Rabatt auf teureres, Rest auf günstigeres
            teureres.rabatt = 99
            verbleibenden_rabatt = benoetigter_rabatt - max_teureres_rabatt
            guenstigeres_prozent = self._berechne_rabatt_prozent(
                verbleibenden_rabatt,
                guenstigeres.praemie
            )
            guenstigeres.rabatt = guenstigeres_prozent

    def _optimiere_rabatte(self):
        """Optimiert die Rabatte iterativ"""
        TOLERANZ = 0.01
        MAX_ITERATIONEN = 100

        for _ in range(MAX_ITERATIONEN):
            aktueller_nettobeitrag = self._berechne_aktuellen_nettobeitrag()
            differenz = aktueller_nettobeitrag - self._zielbeitrag

            # Ziel erreicht
            if abs(differenz) < TOLERANZ:
                break

            # Zu teuer - mehr Rabatt nötig
            if differenz > 0:
                if not self._erhoehe_rabatt():
                    break
            # Zu günstig - Rabatt reduzieren
            else:
                if not self._reduziere_rabatt():
                    break

    def _berechne_aktuellen_nettobeitrag(self) -> float:
        """Berechnet den aktuellen Nettobeitrag"""
        netto_haftpflicht = self._haftpflicht.get_praemie_mit_rabatt()
        netto_kasko = self._kasko.get_praemie_mit_rabatt() if self._kasko else 0.0
        return netto_haftpflicht + netto_kasko

    def _erhoehe_rabatt(self) -> bool:
        """Erhöht den Rabatt um 1%"""
        if self._kasko:
            kasko_ist_teurer = self._kasko.praemie > self._haftpflicht.praemie

            if kasko_ist_teurer:
                if self._kasko.rabatt < 99:
                    self._kasko.rabatt = min(self._kasko.rabatt + 1, 99)
                    return True
                if self._haftpflicht.rabatt < 99:
                    self._haftpflicht.rabatt = min(self._haftpflicht.rabatt + 1, 99)
                    return True
            else:
                if self._haftpflicht.rabatt < 99:
                    self._haftpflicht.rabatt = min(self._haftpflicht.rabatt + 1, 99)
                    return True
                if self._kasko.rabatt < 99:
                    self._kasko.rabatt = min(self._kasko.rabatt + 1, 99)
                    return True
        else:
            if self._haftpflicht.rabatt < 99:
                self._haftpflicht.rabatt = min(self._haftpflicht.rabatt + 1, 99)
                return True

        return False

    def _reduziere_rabatt(self) -> bool:
        """Reduziert den Rabatt um 1%"""
        if self._kasko:
            kasko_ist_teurer = self._kasko.praemie > self._haftpflicht.praemie

            if kasko_ist_teurer:
                # Haftpflicht ist günstiger → erst Haftpflicht reduzieren
                if self._haftpflicht.rabatt > 0:
                    self._haftpflicht.rabatt = max(self._haftpflicht.rabatt - 1, 0)
                    return True
                if self._kasko.rabatt > 0:
                    self._kasko.rabatt = max(self._kasko.rabatt - 1, 0)
                    return True
            else:
                # Kasko ist günstiger → erst Kasko reduzieren
                if self._kasko.rabatt > 0:
                    self._kasko.rabatt = max(self._kasko.rabatt - 1, 0)
                    return True
                if self._haftpflicht.rabatt > 0:
                    self._haftpflicht.rabatt = max(self._haftpflicht.rabatt - 1, 0)
                    return True
        else:
            if self._haftpflicht.rabatt > 0:
                self._haftpflicht.rabatt = max(self._haftpflicht.rabatt - 1, 0)
                return True

        return False