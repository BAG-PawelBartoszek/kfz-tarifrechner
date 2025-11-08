"""Kasko-Produkt"""
from .produkt import Produkt


class Kasko(Produkt):
    """Kasko-Versicherungsprodukt (optional)"""

    def __init__(self, praemie: float, rabatt: int = 0):
        super().__init__(praemie, rabatt)