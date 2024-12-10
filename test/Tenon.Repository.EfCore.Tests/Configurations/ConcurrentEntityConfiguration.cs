using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
///     并发实体配置
/// </summary>
public class ConcurrentEntityConfiguration : AbstractEntityTypeConfiguration<ConcurrentEntity>
{
    public override void Configure(EntityTypeBuilder<ConcurrentEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}