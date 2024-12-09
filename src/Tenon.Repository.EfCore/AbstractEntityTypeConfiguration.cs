using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tenon.Repository.EfCore;

public abstract class AbstractEntityTypeConfiguration
{
    public abstract void Configure(ModelBuilder modelBuilder);
}

public abstract class AbstractEntityTypeConfiguration<TEntity> : AbstractEntityTypeConfiguration,
    IEntityTypeConfiguration<TEntity>
    where TEntity : EfEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        var entityType = typeof(TEntity);
        ConfigureKey(builder, entityType);
        ConfigureQueryFilter(builder, entityType);
        ConfigureConcurrency(builder, entityType);
    }

    public override void Configure(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder.Entity<TEntity>());
    }

    protected void ConfigureQueryFilter(EntityTypeBuilder<TEntity> builder, Type entityType)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnOrder(1).ValueGeneratedNever();
    }

    protected void ConfigureKey(EntityTypeBuilder<TEntity> builder, Type entityType)
    {
        if (typeof(ISoftDelete).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ISoftDelete.IsDeleted))
                .HasDefaultValue(false)
                .HasColumnOrder(2);
            builder.HasQueryFilter(d => !EF.Property<bool>(d, nameof(ISoftDelete.IsDeleted)));
        }
    }

    protected void ConfigureConcurrency(EntityTypeBuilder<TEntity> builder, Type entityType)
    {
        if (typeof(IConcurrency).IsAssignableFrom(entityType))
        {
            builder.Property("RowVersion")
                .IsRequired()
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}