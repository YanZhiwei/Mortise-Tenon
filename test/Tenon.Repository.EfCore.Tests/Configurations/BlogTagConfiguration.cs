using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
/// Blog标签实体配置
/// </summary>
public class BlogTagConfiguration : EfEntityConfigurationBase<BlogTag>
{
    public override void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(DatabaseColumnLength.ShortText);
    }
} 