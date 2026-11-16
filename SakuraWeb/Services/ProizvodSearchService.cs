using SakuraWeb.Models;

namespace SakuraWeb.Services;

public sealed class ProizvodSearchService
{
    public IReadOnlyList<Proizvod> Search(
        IEnumerable<Proizvod> proizvodi,
        string? naziv,
        KategorijaProizvoda? kategorija)
    {
        var rezultat = proizvodi.AsEnumerable();
        var pretraga = naziv?.Trim();

        if (!string.IsNullOrWhiteSpace(pretraga))
        {
            rezultat = rezultat.Where(proizvod =>
                proizvod.naziv.Contains(
                    pretraga,
                    StringComparison.CurrentCultureIgnoreCase));
        }

        if (kategorija.HasValue)
        {
            rezultat = rezultat.Where(proizvod =>
                proizvod.kategorija == kategorija.Value);
        }

        return rezultat.ToList();
    }
}