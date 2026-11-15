using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public class RacunControllerTests
{
    private static Mock<UserManager<Korisnik>> CreateManager()
    {
        return new Mock<UserManager<Korisnik>>(
            Mock.Of<IUserStore<Korisnik>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();

        var controller =
            new RacunController(
                db,
                CreateManager().Object,
                Mock.Of<IEmailSender>());

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForUnknownUser()
    {
        await using var db = TestContext.Create();

        var controller =
            new RacunController(
                db,
                CreateManager().Object,
                Mock.Of<IEmailSender>());

        var result =
            await controller.Details("unknown");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsView()
    {
        await using var db = TestContext.Create();

        var manager = CreateManager();

        var controller =
            new RacunController(
                db,
                manager.Object,
                Mock.Of<IEmailSender>());

        controller.ModelState.AddModelError(
            "email",
            "Neispravan email");

        var result =
            await controller.Create(new Korisnik());

        Assert.IsType<ViewResult>(result);

        manager.Verify(
            item => item.CreateAsync(
                It.IsAny<Korisnik>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_ValidUser_RedirectsAndAssignsRole()
    {
        await using var db = TestContext.Create();

        var manager = CreateManager();

        manager
            .Setup(item => item.CreateAsync(
                It.IsAny<Korisnik>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        manager
            .Setup(item => item.FindByIdAsync(
                It.IsAny<string>()))
            .ReturnsAsync((Korisnik?)null);

        manager
            .Setup(item => item.AddToRoleAsync(
                It.IsAny<Korisnik>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller =
            new RacunController(
                db,
                manager.Object,
                Mock.Of<IEmailSender>());

        var user = new Korisnik
        {
            korisnickoIme = "frizer",
            emailAdresa = "frizer@test.com",
            lozinka = "Password1!",
            ulogaKorisnika = Uloga.Frizer
        };

        var result =
            await controller.Create(user);

        Assert.IsType<RedirectToActionResult>(result);

        manager.Verify(
            item => item.AddToRoleAsync(
                user,
                "Frizer"),
            Times.Once);
    }

    [Fact]
    public async Task ResetPassword_UnknownUser_ReturnsNotFound()
    {
        await using var db = TestContext.Create();

        var manager = CreateManager();

        manager
            .Setup(item => item.FindByIdAsync("missing"))
            .ReturnsAsync((Korisnik?)null);

        var controller =
            new RacunController(
                db,
                manager.Object,
                Mock.Of<IEmailSender>());

        var result =
            await controller.ResetPassword("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}