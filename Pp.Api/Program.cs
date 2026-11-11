using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Pp.Data.Context;
using Pp.Data.Domain;

namespace Pp.Api;
public class Program
{
    public static async Task Main(string[] args)
    {
        if (args is not ["--create-admin"] and not ["--migrate"])
        {
            await CreateHostBuilder(args).Build().RunAsync();
            return;
        }

        using var host = CreateHostBuilder([]).Build();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (args is ["--migrate"])
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("Database is up to date.");
            return;
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var email = configuration["AdminSetup:Email"]?.Trim();
        var password = configuration["AdminSetup:Password"];
        if (string.IsNullOrWhiteSpace(email) || email.Length > 50 || !new EmailAddressAttribute().IsValid(email) ||
            string.IsNullOrWhiteSpace(password) || password.Length < 12 || System.Text.Encoding.UTF8.GetByteCount(password) > 72)
        {
            throw new InvalidOperationException("Configure AdminSetup:Email and AdminSetup:Password (12 characters minimum, 72 bytes maximum).");
        }

        if (await db.Users.AnyAsync(user => user.Role == "Admin" || user.Email == email))
        {
            throw new InvalidOperationException("An administrator or a user with this email already exists.");
        }

        var now = DateTime.UtcNow;
        db.Users.Add(new User
        {
            Name = "Local",
            Surname = "Administrator",
            Email = email,
            Phone = "",
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Admin",
            InsertUser = "System",
            InsertDate = now,
            IsActive = true,
            Wallet = new Wallet { InsertUser = "System", InsertDate = now, IsActive = true }
        });
        await db.SaveChangesAsync();
        Console.WriteLine("Administrator created.");
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
