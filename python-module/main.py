"""Demo-Anwendung für KFZ-Versicherung Tarifierung"""
from src.model import Haftpflicht, Kasko, Tarifierung
from src.common import PlausiException


def main():
    """Hauptprogramm mit Beispielen"""
    print("=== KFZ-Versicherung Tarifierung ===\n")

    try:
        # Beispiel 1: Einfache Tarifierung nur mit Haftpflicht
        print("Beispiel 1: Nur Haftpflicht")
        haftpflicht1 = Haftpflicht(500.0)
        tarif1 = Tarifierung.ohne_zielbeitrag(haftpflicht1)

        print(f"Gesamtprämie: {tarif1.gesamtpraemie:.2f}€")
        print(f"Nettoprämie: {tarif1.nettopraemie:.2f}€")
        print(f"Haftpflicht Rabatt: {tarif1.haftpflicht.rabatt}%\n")

        # Beispiel 2: Tarifierung mit Haftpflicht und Kasko
        print("Beispiel 2: Haftpflicht + Kasko")
        haftpflicht2 = Haftpflicht(500.0)
        kasko2 = Kasko(300.0)
        tarif2 = Tarifierung.ohne_zielbeitrag(haftpflicht2, kasko2)

        print(f"Gesamtprämie: {tarif2.gesamtpraemie:.2f}€")
        print(f"Nettoprämie: {tarif2.nettopraemie:.2f}€")
        print(f"Haftpflicht Rabatt: {tarif2.haftpflicht.rabatt}%")
        print(f"Kasko Rabatt: {tarif2.kasko.rabatt if tarif2.kasko else 'N/A'}%\n")

        # Beispiel 3: Tarifierung mit Zielbeitrag
        print("Beispiel 3: Mit Zielbeitrag von 600€")
        haftpflicht3 = Haftpflicht(500.0)
        kasko3 = Kasko(300.0)
        tarif3 = Tarifierung(600.0, haftpflicht3, kasko3)

        print(f"Zielbeitrag: {tarif3.zielbeitrag:.2f}€")
        print(f"Gesamtprämie: {tarif3.gesamtpraemie:.2f}€")
        print(f"Nettoprämie: {tarif3.nettopraemie:.2f}€")
        print(f"Haftpflicht Rabatt: {tarif3.haftpflicht.rabatt}%")
        print(f"Kasko Rabatt: {tarif3.kasko.rabatt if tarif3.kasko else 'N/A'}%\n")

        # Beispiel 4: Validierungsfehler - negative Prämie
        print("Beispiel 4: Validierungsfehler (wird Exception werfen)")
        haftpflicht_invalid = Haftpflicht(-100.0)
        tarif_invalid = Tarifierung.ohne_zielbeitrag(haftpflicht_invalid)

    except PlausiException as ex:
        print("\nErwarteter Fehler aufgetreten (siehe oben)")

    print("\n=== Programm beendet ===")


if __name__ == "__main__":
    main()