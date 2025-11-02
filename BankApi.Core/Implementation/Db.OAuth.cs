using Microsoft.EntityFrameworkCore;

public class OAuthDb(DbContextOptions<OAuthDb> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<DeviceAuthorization> DeviceAuthorizations { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSeeding((context, _) =>
            {
                User[] usersData = [
                new()
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                    Role = "admin"
                }
                ];
                
                context.Set<User>().AddRange(usersData);
                context.SaveChanges();
            });
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.UseOpenIddict();
    }
}