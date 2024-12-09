using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
/// Blog实体配置
/// </summary>
public class BlogConfiguration : AbstractEntityTypeConfiguration<Blog>
{
    public override void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Content).IsRequired();

        // 添加软删除查询过滤器
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
} 