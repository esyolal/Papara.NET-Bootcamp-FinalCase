using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NUnit.Framework;
using Pp.Data.Context;

namespace Pp.Tests;

public class MigrationTests
{
    [Test]
    public void SqlServerMigrationScriptIncludesCouponAndOrderChanges()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer().Options;
        using var db = new AppDbContext(options);
        var script = db.GetService<IMigrator>().GenerateScript(
            fromMigration: "20240812181620_InitialCreate");

        Assert.That(script, Does.Contain("ALTER COLUMN [UsedByUserId] bigint NULL"));
        Assert.That(script, Does.Contain("CREATE TABLE [Orders]"));
        Assert.That(db.Database.HasPendingModelChanges(), Is.False);
    }
}
