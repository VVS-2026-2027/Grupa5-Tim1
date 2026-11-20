using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed record ModelConstructionCase(
    string PorukaNaziv,
    string PorukaTekst,
    string ProizvodNaziv,
    double ProizvodCijena,
    double ProizvodVolumen,
    string SastojakNaziv,
    string SastojakOpis,
    string SastojakSlika,
    int RezervacijaId,
    DateTime DatumRezervacije,
    DateTime VrijemeTermina,
    bool Otkazana,
    string KlijentId,
    string? FrizerId,
    int UslugaId);

internal static class ModelTestData
{
    public static string TestDataPath(string fileName)
    {
        return Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "TestData",
            fileName);
    }

    public static IEnumerable<object[]> CsvValidationCases(string modelField)
    {
        return File.ReadLines(TestDataPath("model-validation-cases.csv"))
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(',', 3))
            .Where(values => values[0].Trim() == modelField)
            .Select(values => new object[]
            {
                values[1].Trim(),
                bool.Parse(values[2].Trim())
            });
    }

    public static IEnumerable<object[]> JsonConstructionCases()
    {
        var json = File.ReadAllText(TestDataPath("model-construction-cases.json"));
        var cases = JsonSerializer.Deserialize<List<ModelConstructionCase>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return cases!.Select(testCase => new object[] { testCase });
    }

    public static bool IsValid(object model)
    {
        return Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            new List<ValidationResult>(),
            validateAllProperties: true);
    }
}

public sealed class RezervacijaOcjenaCases : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, true };
        yield return new object[] { 5, true };
        yield return new object[] { 0, false };
        yield return new object[] { 6, false };
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
