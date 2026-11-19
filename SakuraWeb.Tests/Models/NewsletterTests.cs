using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public class NewsletterTests
{
    [Fact]
    public void ParameterlessConstructor_SetsSentinelId()
    {
        var newsletter = new Newsletter();

        Assert.Equal(-1, newsletter.id);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(73)]
    public void IdConstructor_SetsId(int id)
    {
        var newsletter = new Newsletter(id);

        Assert.Equal(id, newsletter.id);
    }

    [Theory]
    [InlineData("prazno@test.ba", "")]
    [InlineData("info@sakura.ba", "tajna-lozinka")]
    [InlineData("newsletter+test@example.com", "P@ssword123")]
    public void EmailAndPasswordProperties_AssignedValues_AreRetained(
        string emailAdresa,
        string lozinkaZaEmail)
    {
        var newsletter = new Newsletter
        {
            emailAdresa = emailAdresa,
            lozinkaZaEmail = lozinkaZaEmail
        };

        Assert.Equal(emailAdresa, newsletter.emailAdresa);
        Assert.Equal(lozinkaZaEmail, newsletter.lozinkaZaEmail);
    }
}
