using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
/// BlogTag实体配置
/// </summary>
public class BlogTagConfiguration : AbstractEntityTypeConfiguration<BlogTag>
{
    public override void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasMany(x => x.Blogs)
            .WithMany(x => x.Tags)
            .UsingEntity(j => j.ToTable("BlogTagRelation"));
    }
} 