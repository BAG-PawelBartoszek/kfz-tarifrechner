"""Haftpflicht-Produkt"""
from .produkt import Produkt


class Haftpflicht(Produkt):
    """Haftpflicht-Versicherungsprodukt (obligatorisch)"""

    def __init__(self, praemie: float, rabatt: int = 0):
        super().__init__(praemie, rabatt)