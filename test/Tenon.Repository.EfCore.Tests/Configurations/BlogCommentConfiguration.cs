using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenon.Repository;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Configurations;

/// <summary>
/// Blog评论实体配置
/// </summary>
public class BlogCommentConfiguration : EfEntityConfigurationBase<BlogComment>
{
    public override void Configure(EntityTypeBuilder<BlogComment> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(DatabaseColumnLength.LongText);
        builder.HasOne(x => x.Blog)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.BlogId)
            .IsRequired();
    }
}