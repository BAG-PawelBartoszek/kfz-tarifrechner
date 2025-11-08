"""Tests für die grundlegende Tarifierung"""
import pytest
from src.model import Haftpflicht, Kasko, Tarifierung


class TestTarifierung:
    """Tests für Tarifierungs-Berechnungen ohne Zielbeitrag"""

    def test_berechnung_ohne_zielbeitrag_ohne_kasko_kein_rabatt(self):
        """Berechnung ohne Zielbeitrag, ohne Rabatt und ohne Kasko"""
        haftpflicht = Haftpflicht(75)
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert tarifierung.zielbeitrag is None
        assert tarifierung.gesamtpraemie == 75
        assert tarifierung.nettopraemie == 75

    def test_berechnung_ohne_zielbeitrag_mit_kasko_keine_rabatte(self):
        """Berechnung ohne Zielbeitrag, ohne Rabatt und mit Kasko"""
        kasko = Kasko(50)
        haftpflicht = Haftpflicht(75)
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert tarifierung.zielbeitrag is None
        assert tarifierung.gesamtpraemie == 125
        assert tarifierung.nettopraemie == 125

    def test_berechnung_mit_haftpflicht_rabatt(self):
        """Berechnung mit Haftpflicht-Rabatt"""
        haftpflicht = Haftpflicht(100, 20)  # 20% Rabatt
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert tarifierung.zielbeitrag is None
        assert tarifierung.gesamtpraemie == 100
        assert tarifierung.nettopraemie == 80  # 100 - 20%

    def test_berechnung_mit_kasko_rabatt(self):
        """Berechnung mit Kasko-Rabatt"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50, 10)  # 10% Rabatt
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert tarifierung.zielbeitrag is None
        assert tarifierung.gesamtpraemie == 150
        assert tarifierung.nettopraemie == 145  # 100 + (50 - 5)

    def test_berechnung_mit_haftpflicht_und_kasko_rabatt(self):
        """Berechnung mit beiden Rabatten"""
        haftpflicht = Haftpflicht(100, 20)  # 20% Rabatt
        kasko = Kasko(50, 10)  # 10% Rabatt
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert tarifierung.zielbeitrag is None
        assert tarifierung.gesamtpraemie == 150
        assert tarifierung.nettopraemie == 125  # 80 + 45