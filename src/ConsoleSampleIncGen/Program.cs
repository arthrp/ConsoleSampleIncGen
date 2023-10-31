using ConsoleSampleIncGen.Dto;
using static ConsoleSampleIncGen.Dto.CountryExtensions;

namespace ConsoleSampleIncGen;

public class Program
{
    static void Main(string[] args)
    {
        var c = new CountryParser();
        var x = c.Parse("country.csv");
        
       x[1].PrintProperties();
    }
}
