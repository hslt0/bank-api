using Microsoft.EntityFrameworkCore;

public class OAuthDb(DbContextOptions<OAuthDb> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.UseOpenIddict();
    }
}