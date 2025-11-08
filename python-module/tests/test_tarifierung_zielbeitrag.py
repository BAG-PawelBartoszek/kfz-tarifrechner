"""Tests für Zielbeitrags-Berechnungen"""
import pytest
from src.model import Haftpflicht, Kasko, Tarifierung
from src.common import PlausiException


class TestTarifierungZielbeitrag:
    """Tests für Zielbeitrags-Berechnungen"""

    def test_zielbeitrag_haftpflicht_teurer_nur_haftpflicht_rabatt(self):
        """Zielbeitrag erreicht durch Haftpflicht-Rabatt (Haftpflicht teurer)"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)
        tarifierung = Tarifierung(120, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert abs(tarifierung.nettopraemie - 120) < 0.5

        # Haftpflicht sollte rabattiert sein (da teurer)
        assert tarifierung.haftpflicht.rabatt > 0

        # Gesamtrabatt in % sollte minimal sein
        gesamt_rabatt_prozent = (tarifierung.haftpflicht.rabatt +
                                  tarifierung.kasko.rabatt)
        assert gesamt_rabatt_prozent < 40

    def test_zielbeitrag_kasko_teurer_nur_kasko_rabatt(self):
        """Zielbeitrag erreicht durch Kasko-Rabatt (Kasko teurer)"""
        haftpflicht = Haftpflicht(50)
        kasko = Kasko(100)
        tarifierung = Tarifierung(120, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert abs(tarifierung.nettopraemie - 120) < 0.5

        # Kasko sollte rabattiert sein (da teurer)
        assert tarifierung.kasko.rabatt > 0

        # Gesamtrabatt in % sollte minimal sein
        gesamt_rabatt_prozent = (tarifierung.haftpflicht.rabatt +
                                  tarifierung.kasko.rabatt)
        assert gesamt_rabatt_prozent < 40

    def test_zielbeitrag_vergleich_strategie(self):
        """Vergleich: Teurer zuerst spart Prozentpunkte"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)
        tarifierung = Tarifierung(120, haftpflicht, kasko)

        # Haftpflicht (teurer) sollte ~30% Rabatt haben
        assert 20 <= tarifierung.haftpflicht.rabatt <= 35
        assert tarifierung.kasko.rabatt < 10

        # Gesamtrabatt in Prozentpunkten sollte ~30 sein, nicht ~60
        gesamt_rabatt_prozent = (tarifierung.haftpflicht.rabatt +
                                  tarifierung.kasko.rabatt)
        assert gesamt_rabatt_prozent < 40

    def test_zielbeitrag_gleiche_praemien(self):
        """Zielbeitrag mit gleichen Prämien"""
        haftpflicht = Haftpflicht(75)
        kasko = Kasko(75)
        tarifierung = Tarifierung(120, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert abs(tarifierung.nettopraemie - 120) < 0.5
        assert tarifierung.haftpflicht.rabatt > 0

    def test_zielbeitrag_beide_rabatte_haftpflicht_teurer(self):
        """Zielbeitrag erfordert beide Rabatte (Haftpflicht teurer)"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)
        tarifierung = Tarifierung(60, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert abs(tarifierung.nettopraemie - 60) < 1.0

        # Haftpflicht sollte sehr stark rabattiert sein
        assert tarifierung.haftpflicht.rabatt > 70

        # Die Nettosumme muss stimmen
        netto_gesamt = (tarifierung.haftpflicht.get_praemie_mit_rabatt() +
                        tarifierung.kasko.get_praemie_mit_rabatt())
        assert abs(netto_gesamt - 60) < 1.0

    def test_zielbeitrag_beide_rabatte_kasko_teurer(self):
        """Zielbeitrag erfordert beide Rabatte (Kasko teurer)"""
        haftpflicht = Haftpflicht(50)
        kasko = Kasko(100)
        tarifierung = Tarifierung(60, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert abs(tarifierung.nettopraemie - 60) < 1.0

        # Kasko (teurer) wird zuerst rabattiert
        assert tarifierung.kasko.rabatt > 80

        # Die Nettosumme muss stimmen
        netto_gesamt = (tarifierung.haftpflicht.get_praemie_mit_rabatt() +
                        tarifierung.kasko.get_praemie_mit_rabatt())
        assert abs(netto_gesamt - 60) < 1.0

    def test_zielbeitrag_gleich_gesamtpraemie(self):
        """Zielbeitrag gleich Gesamtprämie - kein Rabatt nötig"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)
        tarifierung = Tarifierung(150, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert tarifierung.nettopraemie == 150
        assert tarifierung.haftpflicht.rabatt == 0
        assert tarifierung.kasko.rabatt == 0

    def test_zielbeitrag_hoeher_als_gesamtpraemie(self):
        """Zielbeitrag höher als Gesamtprämie - kein Rabatt nötig"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)
        tarifierung = Tarifierung(200, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert tarifierung.nettopraemie == 150
        assert tarifierung.haftpflicht.rabatt == 0
        assert tarifierung.kasko.rabatt == 0

    def test_zielbeitrag_nur_haftpflicht(self):
        """Zielbeitrag nur mit Haftpflicht (ohne Kasko)"""
        haftpflicht = Haftpflicht(100)
        tarifierung = Tarifierung(80, haftpflicht, None)

        assert tarifierung.gesamtpraemie == 100
        assert abs(tarifierung.nettopraemie - 80) < 0.5
        assert 15 <= tarifierung.haftpflicht.rabatt <= 25

    def test_zielbeitrag_nicht_erreichbar(self):
        """Zielbeitrag nicht erreichbar - Exception"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)

        # Minimaler Beitrag bei 99% Rabatt: 150 * 0.01 = 1.5
        with pytest.raises(PlausiException) as exc_info:
            Tarifierung(1.0, haftpflicht, kasko)

        assert "nicht erreichbar" in str(exc_info.value)
        assert "Minimaler Beitrag" in str(exc_info.value)

    def test_zielbeitrag_sehr_niedrig(self):
        """Sehr niedriger Zielbeitrag (aber erreichbar)"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(50)
        tarifierung = Tarifierung(2.0, haftpflicht, kasko)

        assert tarifierung.gesamtpraemie == 150
        assert abs(tarifierung.nettopraemie - 2.0) < 0.5

        # Beide sollten nahe am maximalen Rabatt sein
        assert tarifierung.haftpflicht.rabatt >= 98
        assert tarifierung.kasko.rabatt >= 98

    def test_zielbeitrag_minimaler_prozent_rabatt_haftpflicht_teurer(self):
        """Optimierung minimiert Prozent-Rabatt (Haftpflicht teurer)"""
        haftpflicht = Haftpflicht(100)
        kasko = Kasko(20)
        tarifierung = Tarifierung(110, haftpflicht, kasko)

        # 10€ Rabatt nötig: Haftpflicht mit 10% = besser als Kasko mit 50%
        assert tarifierung.haftpflicht.rabatt > 0
        assert tarifierung.kasko.rabatt <= 5

        gesamt_rabatt_prozent = (tarifierung.haftpflicht.rabatt +
                                  tarifierung.kasko.rabatt)
        assert gesamt_rabatt_prozent < 20

    def test_zielbeitrag_minimaler_prozent_rabatt_kasko_teurer(self):
        """Optimierung minimiert Prozent-Rabatt (Kasko teurer)"""
        haftpflicht = Haftpflicht(20)
        kasko = Kasko(100)
        tarifierung = Tarifierung(110, haftpflicht, kasko)

        # 10€ Rabatt nötig: Kasko mit 10% = besser als Haftpflicht mit 50%
        assert tarifierung.kasko.rabatt > 0
        assert tarifierung.haftpflicht.rabatt <= 5

        gesamt_rabatt_prozent = (tarifierung.haftpflicht.rabatt +
                                  tarifierung.kasko.rabatt)
        assert gesamt_rabatt_prozent < 20