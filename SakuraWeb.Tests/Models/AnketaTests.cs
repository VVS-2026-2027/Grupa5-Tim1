using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public class AnketaTests
{
    [Fact]
    public void ParameterlessConstructor_SetsSentinelId()
    {
        var anketa = new Anketa();

        Assert.Equal(-1, anketa.id);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(42)]
    public void IdConstructor_SetsId(int id)
    {
        var anketa = new Anketa(id);

        Assert.Equal(id, anketa.id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(100)]
    public void PoeniProperty_AssignedValue_IsRetained(int poeni)
    {
        var anketa = new Anketa
        {
            poeni = poeni
        };

        Assert.Equal(poeni, anketa.poeni);
    }

    [Fact]
    public void PopunjavanjeProperty_AssignedState_IsRetained()
    {
        var vrijeme = new DateTime(2026, 6, 12, 14, 30, 0);
        var korisnik = new Korisnik { Id = "korisnik-1" };
        var anketa = new Anketa
        {
            vrijemePopunjavanja = vrijeme,
            korisnikId = korisnik.Id,
            korisnik = korisnik
        };

        Assert.Equal(vrijeme, anketa.vrijemePopunjavanja);
        Assert.Equal("korisnik-1", anketa.korisnikId);
        Assert.Same(korisnik, anketa.korisnik);
    }
}
