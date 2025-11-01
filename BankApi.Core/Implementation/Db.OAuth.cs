using Microsoft.EntityFrameworkCore;

public class OAuthDb(DbContextOptions<OAuthDb> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.UseOpenIddict();
    }
}