using Microsoft.EntityFrameworkCore;
using Legaly.Web.Models;

namespace Legaly.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}