
namespace DotnetModule.Model;

public class Haftpflicht : Produkt
{
    public Haftpflicht(double praemie, int rabatt = 0) 
        : base(praemie, rabatt)
    {
    }
}
