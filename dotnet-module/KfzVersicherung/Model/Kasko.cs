namespace DotnetModule.Model;

public class Kasko : Produkt
{
    public Kasko(double praemie, int rabatt = 0) 
        : base(praemie, rabatt)
    {
    }
}
