using System.ComponentModel.DataAnnotations;
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public class PitanjeTests
{
    [Fact]
    public void ParameterlessConstructor_SetsEmptyContentAndAnswerCollection()
    {
        var pitanje = new Pitanje();

        Assert.Equal(string.Empty, pitanje.sadrzaj);
        Assert.Empty(pitanje.odgovori);
        Assert.NotNull(pitanje.odgovori);
    }

    [Theory]
    [InlineData("Kako često negaš kosu?")]
    [InlineData("Koristiš li proizvode za farbanu kosu?")]
    [InlineData("")]
    public void ParameterizedConstructor_SetsContent(string sadrzaj)
    {
        var pitanje = new Pitanje(sadrzaj);

        Assert.Equal(sadrzaj, pitanje.sadrzaj);
    }

    [Fact]
    public void OdgovoriProperty_NewPitanja_HaveIndependentCollections()
    {
        var prvoPitanje = new Pitanje("Prvo pitanje");
        var drugoPitanje = new Pitanje("Drugo pitanje");

        prvoPitanje.odgovori.Add(new Odgovor("Prvi odgovor", 1));

        Assert.Single(prvoPitanje.odgovori);
        Assert.Empty(drugoPitanje.odgovori);
    }

    [Fact]
    public void AnketaAndContentProperties_AssignedValues_AreRetained()
    {
        var anketa = new Anketa(12);
        var pitanje = new Pitanje
        {
            anketaId = anketa.id,
            anketa = anketa,
            sadrzaj = "Koji stil preferiraš?"
        };

        Assert.Equal(12, pitanje.anketaId);
        Assert.Same(anketa, pitanje.anketa);
        Assert.Equal("Koji stil preferiraš?", pitanje.sadrzaj);
    }

    [Fact]
    public void SadrzajProperty_EmptyValue_IsInvalid()
    {
        var pitanje = new Pitanje
        {
            sadrzaj = string.Empty
        };

        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            pitanje,
            new ValidationContext(pitanje),
            results,
            validateAllProperties: true);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Pitanje.sadrzaj)));
    }
}
