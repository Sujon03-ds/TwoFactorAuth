using Microsoft.EntityFrameworkCore;
using TwoFactorAuth.Models;

namespace TwoFactorAuth.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectSection> ProjectSections { get; set; }
    }
}
