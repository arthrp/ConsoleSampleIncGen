using ConsoleSampleIncGen.Dto;

namespace ConsoleSampleIncGen;

public class CountryParser
{
    public List<Country> Parse(string fileName)
    {
        var result = new List<Country>();
        var lines = File.ReadAllLines(fileName);
        foreach (var line in lines)
        {
            var vals = line.Split(';');
            result.Add(new Country(){ CountryCode = vals[0], Name = vals[1], Continent = vals[3], Capital = vals[4]});
        }

        return result;
    }
}