using DentalClinic.API.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Data;

public static class DatabaseSeeder
{
    private const string DemoAdminEmail = "admin@demo.com";
    private const string DemoAdminPassword = "Admin@123";

    public static async Task SeedDevelopmentDataAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DentalClinicDbContext>();

        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            logger.LogWarning("Database connection failed. Skipping seed data.");
            return;
        }

        var clinic = await dbContext.Clinics
            .AsNoTracking()
            .OrderBy(c => c.ClinicId)
            .FirstOrDefaultAsync(cancellationToken);

        if (clinic is null)
        {
            logger.LogWarning("No clinic found. Run database/dental_clinic_db.sql first.");
            return;
        }

        var hasUsers = await dbContext.Users.AnyAsync(cancellationToken);
        if (hasUsers)
        {
            logger.LogInformation("Users already exist. Skipping admin seed.");
            return;
        }

        dbContext.Users.Add(new Models.User
        {
            ClinicId = clinic.ClinicId,
            FullName = "Demo Admin",
            Email = DemoAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoAdminPassword),
            Role = "ADMIN",
            Phone = "+970000000001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded demo admin user: {Email} / {Password}",
            DemoAdminEmail,
            DemoAdminPassword);
    }
}
