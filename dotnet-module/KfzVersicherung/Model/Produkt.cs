using System.ComponentModel.DataAnnotations;

namespace DotnetModule.Model;

public abstract class Produkt
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Prämie muss positiv sein")]
    public double Praemie { get; }

    [Range(0, 99, ErrorMessage = "Rabatt muss zwischen 0 und 99 liegen")]
    public int Rabatt { get; set; }

    protected Produkt(double praemie, int rabatt = 0)
    {
        Praemie = praemie;
        Rabatt = rabatt;
    }

    public double GetPraemieMitRabatt()
    {
        return Praemie * ((100 - Rabatt) / 100.0);
    }
}
