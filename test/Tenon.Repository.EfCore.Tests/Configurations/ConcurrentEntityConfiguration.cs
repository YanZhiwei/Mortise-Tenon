using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
/// 并发实体配置
/// </summary>
public class ConcurrentEntityConfiguration : EfEntityConfigurationBase<ConcurrentEntity>
{
    public override void Configure(EntityTypeBuilder<ConcurrentEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(DatabaseColumnLength.NormalText);
    }
}