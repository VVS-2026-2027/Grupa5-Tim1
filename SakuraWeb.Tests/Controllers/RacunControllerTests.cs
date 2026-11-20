using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;
using System.Reflection;

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

    [Fact]
    public async Task Index_ReturnsViewWithAllUsers()
    {
        await using var db = TestContext.Create();
        db.korisnici.AddRange(NewUser("user-1"), NewUser("user-2"));
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var users = Assert.IsAssignableFrom<List<Korisnik>>(view.Model);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var db = TestContext.Create();

        var result = CreateController(db).Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewForExistingUser()
    {
        await using var db = TestContext.Create();
        var user = NewUser("user-1");
        db.korisnici.Add(user);
        await db.SaveChangesAsync();

        var result = await CreateController(db).Details(user.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(user, view.Model);
    }

    [Fact]
    public async Task Create_WhenUserManagerReturnsCreatedUser_UpdatesHashAndRole()
    {
        await using var db = TestContext.Create();
        var manager = CreateManager();
        var user = NewUser("created-user");
        var createdUser = NewUser(user.Id);
        createdUser.PasswordHash = "hashed-password";

        manager.Setup(item => item.CreateAsync(user, user.lozinka))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(item => item.FindByIdAsync(user.Id))
            .ReturnsAsync(createdUser);
        manager.Setup(item => item.UpdateAsync(createdUser))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(item => item.AddToRoleAsync(user, "Frizer"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(db, manager.Object);
        var result = await controller.Create(user);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(user.korisnickoIme, user.UserName);
        Assert.Equal(user.emailAdresa, user.Email);
        Assert.True(user.EmailConfirmed);
        Assert.False(user.jePretplacenNaNewsletter);
        Assert.Equal("hashed-password", createdUser.lozinka);
        manager.Verify(item => item.UpdateAsync(createdUser), Times.Once);
        manager.Verify(item => item.AddToRoleAsync(user, "Frizer"), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUserManagerFails_ReturnsViewWithErrors()
    {
        await using var db = TestContext.Create();
        var manager = CreateManager();
        var user = NewUser("duplicate-user");
        manager.Setup(item => item.CreateAsync(user, user.lozinka))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Description = "User already exists."
            }));

        var result = await CreateController(db, manager.Object).Create(user);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Contains(
            "User already exists.",
            view.ViewData.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage));
        manager.Verify(item => item.AddToRoleAsync(
            It.IsAny<Korisnik>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Get_NullIdReturnsNotFound()
    {
        await using var db = TestContext.Create();

        var result = await CreateController(db).Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_UnknownUserReturnsNotFound()
    {
        await using var db = TestContext.Create();

        var result = await CreateController(db).Edit("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ExistingUserReturnsView()
    {
        await using var db = TestContext.Create();
        var user = NewUser("user-1");
        db.korisnici.Add(user);
        await db.SaveChangesAsync();

        var result = await CreateController(db).Edit(user.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(user, view.Model);
    }

    [Fact]
    public async Task ResetPassword_NullIdReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var manager = CreateManager();

        var result = await CreateController(db, manager.Object)
            .ResetPassword(null!);

        Assert.IsType<NotFoundResult>(result);
        manager.Verify(item => item.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_Success_UpdatesPasswordAndSendsEmail()
    {
        await using var db = TestContext.Create();
        var manager = CreateManager();
        var user = NewUser("user-1");
        var updatedUser = NewUser(user.Id);
        updatedUser.PasswordHash = "new-hash";
        manager.SetupSequence(item => item.FindByIdAsync(user.Id))
            .ReturnsAsync(user)
            .ReturnsAsync(updatedUser);
        manager.Setup(item => item.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        manager.Setup(item => item.ResetPasswordAsync(
                user, "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(item => item.UpdateAsync(updatedUser))
            .ReturnsAsync(IdentityResult.Success);

        string? sentBody = null;
        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(item => item.SendEmailAsync(
                user.emailAdresa, It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, body) => sentBody = body)
            .Returns(Task.CompletedTask);

        var controller = CreateController(db, manager.Object, emailSender.Object);
        SetTempData(controller);
        var result = await controller.ResetPassword(user.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("new-hash", updatedUser.lozinka);
        Assert.NotNull(sentBody);
        var password = sentBody!.Split("glasi: ")[1].Split(". Molimo vas")[0];
        Assert.Equal(12, password.Length);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, character => "!@#$%^&*".Contains(character));
        emailSender.Verify(item => item.SendEmailAsync(
            user.emailAdresa,
            "[Sakura] Reset lozinke",
            It.IsAny<string>()), Times.Once);
        manager.Verify(item => item.UpdateAsync(updatedUser), Times.Once);
        Assert.Contains("uspješno resetovana", controller.TempData["SuccessMessage"]?.ToString());
    }

    [Fact]
    public async Task ResetPassword_FailedReset_RedirectsToEdit()
    {
        await using var db = TestContext.Create();
        var manager = CreateManager();
        var user = NewUser("user-1");
        manager.Setup(item => item.FindByIdAsync(user.Id)).ReturnsAsync(user);
        manager.Setup(item => item.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        manager.Setup(item => item.ResetPasswordAsync(
                user, "reset-token", It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Description = "Reset rejected."
            }));

        var emailSender = new Mock<IEmailSender>();
        var result = await CreateController(db, manager.Object, emailSender.Object)
            .ResetPassword(user.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirect.ActionName);
        Assert.Equal(user.Id, redirect.RouteValues!["id"]);
        emailSender.Verify(item => item.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Get_NullIdReturnsNotFound()
    {
        await using var db = TestContext.Create();

        var result = await CreateController(db).Delete((string?)null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_UnknownUserReturnsNotFound()
    {
        await using var db = TestContext.Create();

        var result = await CreateController(db).Delete("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ExistingUserReturnsView()
    {
        await using var db = TestContext.Create();
        var user = NewUser("user-1");
        db.korisnici.Add(user);
        await db.SaveChangesAsync();

        var result = await CreateController(db).Delete(user.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(user, view.Model);
    }

    [Fact]
    public async Task Delete_Post_ExistingUserDeletesAndSendsNotification()
    {
        await using var db = TestContext.Create();
        var user = NewUser("user-1");
        db.korisnici.Add(user);
        await db.SaveChangesAsync();
        var emailSender = new Mock<IEmailSender>();
        var controller = CreateController(db, emailSender: emailSender.Object);
        SetTempData(controller);

        var result = await controller.Delete(user.Id, "Zahtjev administratora");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.korisnici);
        emailSender.Verify(item => item.SendEmailAsync(
            user.emailAdresa,
            "[Sakura] Vaš račun je obrisan",
            "Administrator je obrisao vaš račun. Razlog: Zahtjev administratora"), Times.Once);
        Assert.Contains("uspješno obrisan", controller.TempData["SuccessMessage"]?.ToString());
    }

    [Fact]
    public async Task Delete_Post_UnknownUserRedirectsWithoutSendingEmail()
    {
        await using var db = TestContext.Create();
        var emailSender = new Mock<IEmailSender>();
        var controller = CreateController(db, emailSender: emailSender.Object);

        var result = await controller.Delete("missing", "reason");

        Assert.IsType<RedirectToActionResult>(result);
        emailSender.Verify(item => item.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task KorisnikExists_ReturnsWhetherUserIsInDatabase()
    {
        await using var db = TestContext.Create();
        db.korisnici.Add(NewUser("user-1"));
        await db.SaveChangesAsync();
        var controller = CreateController(db);
        var method = typeof(RacunController).GetMethod(
            "KorisnikExists", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);
        Assert.True((bool)method.Invoke(controller, new object[] { "user-1" })!);
        Assert.False((bool)method.Invoke(controller, new object[] { "missing" })!);
    }

    private static RacunController CreateController(
        SakuraWeb.Data.ApplicationDbContext db,
        UserManager<Korisnik>? manager = null,
        IEmailSender? emailSender = null)
    {
        return new RacunController(
            db,
            manager ?? CreateManager().Object,
            emailSender ?? Mock.Of<IEmailSender>());
    }

    private static Korisnik NewUser(string id)
    {
        return new Korisnik
        {
            Id = id,
            korisnickoIme = "frizer",
            emailAdresa = "frizer@example.com",
            lozinka = "Password1!",
            UserName = "frizer",
            Email = "frizer@example.com",
            ulogaKorisnika = Uloga.Frizer
        };
    }

    private static void SetTempData(RacunController controller)
    {
        controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }
}