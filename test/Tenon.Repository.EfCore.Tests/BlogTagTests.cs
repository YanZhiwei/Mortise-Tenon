using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

[TestClass]
public class BlogTagTests : TestBase
{
    /// <summary>
    /// 测试创建标签
    /// </summary>
    [TestMethod]
    public async Task CreateTag_ShouldCreateSuccessfully()
    {
        // Arrange
        var tag = new BlogTag
        {
            Name = "新标签",
            Description = "这是一个新标签"
        };

        // Act
        await BlogTagEfRepo.InsertAsync(tag, token: default);
        var savedTag = await BlogTagEfRepo.GetAsync(tag.Id, token: default);

        // Assert
        Assert.IsNotNull(savedTag);
        Assert.AreEqual(tag.Name, savedTag.Name);
        Assert.AreEqual(tag.Description, savedTag.Description);
        Assert.IsTrue(savedTag.CreatedAt > DateTimeOffset.MinValue);
        Assert.IsNull(savedTag.UpdatedAt);
        Assert.IsNull(savedTag.DeletedAt);
    }

    /// <summary>
    /// 测试获取使用最多的标签
    /// </summary>
    [TestMethod]
    public async Task GetMostUsedTags_ShouldReturnOrderedTags()
    {
        // Act
        var tags = await DbContext.BlogTags
            .Include(t => t.Blogs)
            .OrderByDescending(t => t.Blogs.Count)
            .ToListAsync();

        // Assert
        Assert.IsTrue(tags.Count > 0);
        for (int i = 1; i < tags.Count; i++)
        {
            Assert.IsTrue(tags[i - 1].Blogs.Count >= tags[i].Blogs.Count);
        }
    }

    /// <summary>
    /// 测试更新标签
    /// </summary>
    [TestMethod]
    public async Task UpdateTag_ShouldUpdateAuditFields()
    {
        // Arrange
        var tag = await BlogTagEfRepo.GetAsync(1, noTracking: false, token: default);
        Assert.IsNotNull(tag);

        var originalCreatedAt = tag.CreatedAt;

        // Act
        tag.Description = "更新后的描述";
        await BlogTagEfRepo.UpdateAsync(tag, token: default);

        // Assert
        var updatedTag = await BlogTagEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(updatedTag);
        Assert.AreEqual("更新后的描述", updatedTag.Description);
        Assert.AreEqual(originalCreatedAt, updatedTag.CreatedAt);
        Assert.IsTrue(updatedTag.UpdatedAt.HasValue);
    }

    /// <summary>
    /// 测试删除标签
    /// </summary>
    [TestMethod]
    public async Task DeleteTag_ShouldNotDeleteAssociatedBlogs()
    {
        // Arrange
        var tagId = 1L;

        // 获取关联的博客数量
        var blogCount = await BlogEfRepo.CountAsync(b => b.Tags.Any(t => t.Id == tagId), token: default);
        Assert.AreNotEqual(0, blogCount);

        // Act
        var tag = await DbContext.BlogTags.FindAsync(tagId);
        Assert.IsNotNull(tag);
        DbContext.BlogTags.Remove(tag);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedTag = await BlogTagEfRepo.GetAsync(tagId, noTracking: true, token: default);
        Assert.IsNull(deletedTag);

        // 验证关联的博客没有被删除
        var remainingBlogCount = await BlogEfRepo.CountAsync(b => true, token: default);
        Assert.AreEqual(3, remainingBlogCount); // 初始化时创建了3篇博客
    }

    /// <summary>
    /// 测试获取特定标签的所有博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByTag_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var tag = await BlogTagEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(tag);

        // Act
        var blogs = await DbContext.Blogs
            .Include(b => b.Tags)
            .Where(b => b.Tags.Any(t => t.Id == tag.Id))
            .ToListAsync();

        // Assert
        Assert.IsTrue(blogs.Count > 0);
        Assert.IsTrue(blogs.All(b => b.Tags.Any(t => t.Id == tag.Id)));
    }

    /// <summary>
    /// 测试获取没有博客的标签
    /// </summary>
    [TestMethod]
    public async Task GetUnusedTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var newTag = new BlogTag { Name = "未使用标签", Description = "这个标签还没有被使用" };
        await BlogTagEfRepo.InsertAsync(newTag, token: default);

        // Act
        var unusedTags = await DbContext.BlogTags
            .Include(t => t.Blogs)
            .Where(t => !t.Blogs.Any())
            .ToListAsync();

        // Assert
        Assert.IsTrue(unusedTags.Count > 0);
        Assert.IsTrue(unusedTags.All(t => t.Blogs.Count == 0));
    }
}