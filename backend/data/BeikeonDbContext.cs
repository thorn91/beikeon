using api.domain;
using api.domain.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace api.data;

public class BeikeonDbContext : DbContext {
    private static readonly Predicate<EntityEntry>
        HasCreatedTime = e => e.GetType().GetProperty("CreatedTime") is not null;

    private static readonly Predicate<EntityEntry> NeedsLastUpdateTime = e =>
        e.Entity.GetType().GetProperty("LastUpdateTime") != null &&
        e.State is EntityState.Modified or EntityState.Added;

    public BeikeonDbContext() { }

    public BeikeonDbContext(DbContextOptions<BeikeonDbContext> options) : base(options) { }

    public DbSet<User>? Users { get; set; }

    public override int SaveChanges() {
        /* Note that this will technically make the created times slightly off, but until we have a legal
         * department that tells me not to, I'd rather not hammer that function call. */
        var currentTime = DateTime.UtcNow;
        var entries = ChangeTracker.Entries();
        var entityEntries = entries as EntityEntry[] ?? entries.ToArray();

        SetLastUpdateTime(entityEntries, currentTime);
        SetCreatedTimeIfNeeded(entityEntries, currentTime);

        return base.SaveChanges();
    }

    private static void SetCreatedTimeIfNeeded(IEnumerable<EntityEntry> entries, DateTime currentTime) {
        entries.Where(e => HasCreatedTime(e)).ToList()
            .ForEach(e => {
                if (e.State == EntityState.Added) e.Property("CreatedTime").CurrentValue = currentTime;
                if (e.State == EntityState.Modified) e.Property("CreatedTime").IsModified = false;
            });
    }

    private static void SetLastUpdateTime(IEnumerable<EntityEntry> entries, DateTime currentTime) {
        entries.Where(e => NeedsLastUpdateTime(e)).ToList()
            .ForEach(e => e.Property("LastUpdateTime").CurrentValue = currentTime);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        ConfigureTablePerConcreteForAbstractBase(modelBuilder);
    }

    private void ConfigureTablePerConcreteForAbstractBase(ModelBuilder modelBuilder) {
        modelBuilder.Entity<AbstractDomainEntity>().UseTpcMappingStrategy();
    }
}