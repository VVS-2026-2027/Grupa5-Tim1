using Microsoft.EntityFrameworkCore;
using SakuraWeb.Data;

namespace SakuraWeb.Tests.Helpers;

public static class TestContext
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}