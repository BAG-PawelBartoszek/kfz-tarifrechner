
namespace DotnetModule.Common;

public class PlausiException : Exception
{
    public List<string> Fehler { get; }

    public PlausiException(List<string> fehler) 
        : base(ErstelleFehlermeldung(fehler))
    {
        Fehler = fehler;
        // Ausgabe in Console
        Console.Error.WriteLine(Message);
    }

    private static string ErstelleFehlermeldung(List<string> fehler)
    {
        if (fehler.Count == 0)
        {
            return "Plausibilitätsprüfung fehlgeschlagen";
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine();
        sb.AppendLine(new string('=', 70));
        sb.AppendLine("PLAUSIBILITÄTSFEHLER");
        sb.AppendLine(new string('=', 70));
        
        for (int i = 0; i < fehler.Count; i++)
        {
            sb.AppendLine($"  {i + 1}. {fehler[i]}");
        }
        
        sb.Append(new string('=', 70));
        
        return sb.ToString();
    }
}
