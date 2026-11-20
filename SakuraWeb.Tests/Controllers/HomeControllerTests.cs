using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public sealed class HomeControllerTests
{
    [Fact]
    public void Index_EmptyDatabase_ReturnsZeroCounts()
    {
        using var db = TestContext.Create();
        var controller = new HomeController(db);

        var result = Assert.IsType<ViewResult>(controller.Index());
        var model = Assert.IsType<HomeIndexViewModel>(result.Model);

        Assert.Equal(0, model.KorisniciCount);
        Assert.Equal(0, model.RezervacijeCount);
        Assert.Equal(0, model.ProizvodiCount);
    }

    [Fact]
    public void Index_PopulatedDatabase_ReturnsEntityCounts()
    {
        using var db = TestContext.Create();
        db.korisnici.AddRange(
            new Korisnik { Id = "client-1", korisnickoIme = "Klijent 1" },
            new Korisnik { Id = "client-2", korisnickoIme = "Klijent 2" });
        db.proizvodi.AddRange(
            new Proizvod { id = 1, naziv = "Sampon" },
            new Proizvod { id = 2, naziv = "Ulje" },
            new Proizvod { id = 3, naziv = "Maska" });
        db.SaveChanges();

        var result = Assert.IsType<ViewResult>(new HomeController(db).Index());
        var model = Assert.IsType<HomeIndexViewModel>(result.Model);

        Assert.Equal(2, model.KorisniciCount);
        Assert.Equal(0, model.RezervacijeCount);
        Assert.Equal(3, model.ProizvodiCount);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        using var db = TestContext.Create();
        var controller = new HomeController(db);

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_UsesHttpTraceIdentifierWhenActivityIdIsUnavailable()
    {
        using var db = TestContext.Create();
        var controller = new HomeController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.TraceIdentifier = "request-123";

        var result = Assert.IsType<ViewResult>(controller.Error());
        var model = Assert.IsType<ErrorViewModel>(result.Model);

        Assert.Equal("request-123", model.RequestId);
        Assert.True(model.ShowRequestId);
    }
}
