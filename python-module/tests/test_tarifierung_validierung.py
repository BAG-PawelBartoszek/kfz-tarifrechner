"""Tests für die Validierung der Tarifierung"""
import pytest
from src.model import Haftpflicht, Kasko, Tarifierung
from src.common import PlausiException


class TestTarifierungValidierung:
    """Tests für Validierungsfehler"""

    def test_haftpflicht_null_wirft_exception(self):
        """Haftpflicht null - sollte PlausiException werfen"""
        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(None)

        assert "Haftpflicht ist Pflichtfeld" in str(exc_info.value)

    def test_haftpflicht_praemie_negativ_wirft_exception(self):
        """Haftpflicht Prämie negativ - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(-100)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert "Prämie für die Haftpflicht muss positiv sein" in str(exc_info.value)

    def test_haftpflicht_praemie_null_wirft_exception(self):
        """Haftpflicht Prämie null (0) - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(0)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert "Prämie für die Haftpflicht muss positiv sein" in str(exc_info.value)

    def test_haftpflicht_rabatt_negativ_wirft_exception(self):
        """Haftpflicht Rabatt negativ - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100, -10)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert "Rabatt für die Haftpflicht darf nicht negativ sein" in str(exc_info.value)

    def test_haftpflicht_rabatt_100_wirft_exception(self):
        """Haftpflicht Rabatt >= 100 - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100, 100)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert "Rabatt für die Haftpflicht darf nicht größer 99 sein" in str(exc_info.value)

    def test_haftpflicht_rabatt_ueber_100_wirft_exception(self):
        """Haftpflicht Rabatt > 100 - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100, 150)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert "Rabatt für die Haftpflicht darf nicht größer 99 sein" in str(exc_info.value)

    def test_kasko_praemie_negativ_wirft_exception(self):
        """Kasko Prämie negativ - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(-50)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert "Prämie für die Kasko muss positiv sein" in str(exc_info.value)

    def test_kasko_praemie_null_wirft_exception(self):
        """Kasko Prämie null (0) - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(0)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert "Prämie für die Kasko muss positiv sein" in str(exc_info.value)

    def test_kasko_rabatt_negativ_wirft_exception(self):
        """Kasko Rabatt negativ - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50, -10)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert "Rabatt für die Kasko darf nicht negativ sein" in str(exc_info.value)

    def test_kasko_rabatt_100_wirft_exception(self):
        """Kasko Rabatt >= 100 - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50, 100)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert "Rabatt für die Kasko darf nicht größer 99 sein" in str(exc_info.value)

    def test_zielbeitrag_negativ_wirft_exception(self):
        """Zielbeitrag negativ - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung(-200, haftpflicht, kasko)

        expected_message = "  1. Zielbeitrag von -200.00€ ist nicht erreichbar. Minimaler Beitrag bei maximalem Rabatt (99%): 1.50€"
        actual_message = str(exc_info.value).split('\n')[4]  # Extract the error message from the exception string
        assert actual_message in expected_message

    def test_zielbeitrag_null_wirft_exception(self):
        """Zielbeitrag null (0) - sollte PlausiException werfen"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung(0, haftpflicht, kasko)

        expected_message = "  1. Zielbeitrag von 0.00€ ist nicht erreichbar. Minimaler Beitrag bei maximalem Rabatt (99%): 1.50€"
        actual_message = str(exc_info.value).split('\n')[4]  # Extract the error message from the exception string
        assert actual_message in expected_message

    def test_multiple_fehler_sammelt_alle_fehler(self):
        """Multiple Fehler - sollte alle Fehler in PlausiException sammeln"""
        haftpflicht = Haftpflicht(-100, -10)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert len(exc_info.value.fehler) == 2
        assert "Prämie für die Haftpflicht muss positiv sein" in exc_info.value.fehler
        assert "Rabatt für die Haftpflicht darf nicht negativ sein" in exc_info.value.fehler

    def test_haftpflicht_und_kasko_multiple_fehler_sammelt_alle(self):
        """Haftpflicht und Kasko mit mehreren Fehlern"""
        haftpflicht = Haftpflicht(-100, 150)
        kasko = Kasko(-50, -20)

        with pytest.raises(PlausiException) as exc_info:
            Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert len(exc_info.value.fehler) == 4

    def test_gueltige_haftpflicht_ohne_kasko_keine_exception(self):
        """Gültige Haftpflicht ohne Kasko - keine Exception"""
        haftpflicht = Haftpflicht(100, 10)
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht)

        assert tarifierung is not None
        assert tarifierung.haftpflicht == haftpflicht
        assert tarifierung.kasko is None

    def test_rabatt_0_ist_gueltig(self):
        """Rabatt 0 ist gültig"""
        haftpflicht = Haftpflicht(100, 0)
        kasko = Kasko(50, 0)
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert tarifierung.haftpflicht.rabatt == 0
        assert tarifierung.kasko.rabatt == 0

    def test_rabatt_99_ist_gueltig(self):
        """Rabatt 99 ist gültig (Grenzwert)"""
        haftpflicht = Haftpflicht(100, 99)
        kasko = Kasko(50, 99)
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert tarifierung.haftpflicht.rabatt == 99
        assert tarifierung.kasko.rabatt == 99

    def test_sehr_kleine_praemie_ist_gueltig(self):
        """Sehr kleine Prämie (0.01) ist gültig"""
        haftpflicht = Haftpflicht(0.01, 0)
        kasko = Kasko(0.01, 0)
        tarifierung = Tarifierung.ohne_zielbeitrag(haftpflicht, kasko)

        assert tarifierung.haftpflicht.praemie == 0.01
        assert tarifierung.kasko.praemie == 0.01