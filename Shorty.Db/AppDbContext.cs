using Microsoft.EntityFrameworkCore;
using Shorty.Core;

namespace Shorty.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UrlGroup> UrlGroups { get; set; } = null!;
    public DbSet<Url> Urls { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}