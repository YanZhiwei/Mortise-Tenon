using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
///     Blog实体配置
/// </summary>
public class BlogConfiguration : EfEntityConfigurationBase<Blog>
{
    public override void Configure(EntityTypeBuilder<Blog> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(DatabaseColumnLength.NormalText);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(DatabaseColumnLength.ExtendedText);
    }
}