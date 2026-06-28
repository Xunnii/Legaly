using Microsoft.EntityFrameworkCore;
using Legaly.Web.Models.Entities;

namespace Legaly.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.NomorSurat)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .Property(d => d.Harga)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Document>()
            .HasOne(d => d.Template)
            .WithMany()
            .HasForeignKey(d => d.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
