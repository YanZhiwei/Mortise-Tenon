using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
/// BlogComment实体配置
/// </summary>
public class BlogCommentConfiguration : AbstractEntityTypeConfiguration<BlogComment>
{
    public override void Configure(EntityTypeBuilder<BlogComment> builder)
    {
        builder.Property(x => x.Content).IsRequired();
        builder.HasOne(x => x.Blog)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.BlogId)
            .IsRequired();

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}