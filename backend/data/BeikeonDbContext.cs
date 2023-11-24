using api.domain.user;
using Microsoft.EntityFrameworkCore;

namespace api.data;

public class BeikeonDbContext : DbContext {
    public BeikeonDbContext() { }

    public BeikeonDbContext(DbContextOptions<BeikeonDbContext> options) : base(options) { }

    public DbSet<User>? Users { get; set; }
}