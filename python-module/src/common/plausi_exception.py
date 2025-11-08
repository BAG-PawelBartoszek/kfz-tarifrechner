"""Plausibilitätsprüfungs-Exception für Validierungsfehler"""
import sys
from typing import List


class PlausiException(Exception):
    """Exception für Plausibilitätsfehler bei der Validierung"""

    def __init__(self, fehler: List[str]):
        self.fehler = fehler
        message = self._erstelle_fehlermeldung(fehler)
        super().__init__(message)
        # Ausgabe in Console (wie in C# und Java)
        print(message, file=sys.stderr)

    @staticmethod
    def _erstelle_fehlermeldung(fehler: List[str]) -> str:
        """Erstellt eine formatierte Fehlermeldung"""
        if not fehler:
            return "Plausibilitätsprüfung fehlgeschlagen"

        lines = [
            "",
            "=" * 70,
            "PLAUSIBILITÄTSFEHLER",
            "=" * 70
        ]

        for i, fehler_text in enumerate(fehler, 1):
            lines.append(f"  {i}. {fehler_text}")

        lines.append("=" * 70)

        return "\n".join(lines)