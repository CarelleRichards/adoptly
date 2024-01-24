using Microsoft.EntityFrameworkCore;
using Adoptly.Web.Models;

namespace Adoptly.Web.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Login> Logins { get; set; }
    public DbSet<Adopter> Adopters { get; set; }
    public DbSet<Shelter> Shelters { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<Quiz> Quiz { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Make tables per concrete type (TPC) of User.
        builder.Entity<User>().UseTpcMappingStrategy();
        builder.Entity<Adopter>().ToTable("Adopters");
        builder.Entity<Shelter>().ToTable("Shelters");
        builder.Entity<Admin>().ToTable("Admins");

        builder.Entity<Pet>().Property(x => x.AnimalType).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.AnimalType).HasMaxLength(3);

        builder.Entity<Pet>().Property(x => x.State).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.State).HasMaxLength(3);

        builder.Entity<Pet>().Property(x => x.Sex).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.Sex).HasMaxLength(6);

        builder.Entity<Pet>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.Status).HasMaxLength(11);

        builder.Entity<Pet>().Property(x => x.Colour).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.Colour).HasMaxLength(6);

        builder.Entity<Pet>().Property(x => x.Independence).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.Independence).HasMaxLength(6);

        builder.Entity<Pet>().Property(x => x.ActivityLevel).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.ActivityLevel).HasMaxLength(6);

        builder.Entity<Pet>().Property(x => x.Budget).HasConversion<string>();
        builder.Entity<Pet>().Property(x => x.Budget).HasMaxLength(6);

        builder.Entity<Shelter>().Property(x => x.State).HasConversion<string>();
        builder.Entity<Shelter>().Property(x => x.State).HasMaxLength(3);

        builder.Entity<Adopter>().Property(x => x.State).HasConversion<string>();
        builder.Entity<Adopter>().Property(x => x.State).HasMaxLength(3);
        
        builder.Entity<Application>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Application>().Property(x => x.Status).HasMaxLength(10);
        
        builder.Entity<Address>().Property(x => x.State).HasConversion<string>();
        builder.Entity<Address>().Property(x => x.State).HasMaxLength(3);

        base.OnModelCreating(builder);
    }
}