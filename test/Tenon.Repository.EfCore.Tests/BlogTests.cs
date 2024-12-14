using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

[TestClass]
public class BlogTests : TestBase
{
    #region Create Tests

    /// <summary>
    /// 测试创建博客（不带标签）
    /// </summary>
    [TestMethod]
    public async Task CreateBlog_WithoutTags_ShouldCreateSuccessfully()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "测试博客",
            Content = "这是一篇测试博客",
            Author = "测试作者",
            PublishTime = DateTime.Now
        };

        // Act
        await BlogEfRepo.InsertAsync(blog, token: default);

        // Assert
        var savedBlog = await BlogEfRepo.GetAsync(blog.Id, token: default);
        Assert.IsNotNull(savedBlog);
        Assert.AreEqual(blog.Title, savedBlog.Title);
        Assert.AreEqual(blog.Content, savedBlog.Content);
        Assert.AreEqual(blog.Author, savedBlog.Author);
        Assert.AreEqual(1, savedBlog.CreatedBy);
        Assert.IsTrue(savedBlog.CreatedAt > DateTimeOffset.MinValue);
        Assert.IsFalse(savedBlog.IsDeleted);
        Assert.IsNull(savedBlog.UpdatedAt);
        Assert.IsNull(savedBlog.UpdatedBy);
        Assert.IsNull(savedBlog.DeletedAt);
        Assert.IsNull(savedBlog.DeletedBy);
    }

    /// <summary>
    /// 测试创建博客（带标签）
    /// </summary>
    [TestMethod]
    public async Task CreateBlog_WithTags_ShouldCreateSuccessfully()
    {
        // Arrange
        var tags = new List<BlogTag>
        {
            new() { Name = "技术", Description = "技术相关文章" },
            new() { Name = "编程", Description = "编程技巧" }
        };
        await BlogTagEfRepo.InsertAsync(tags, token: default);

        var blog = new Blog
        {
            Title = "测试博客",
            Content = "这是一篇测试博客",
            Author = "测试作者",
            PublishTime = DateTime.Now,
            Tags = tags
        };

        // Act
        await BlogEfRepo.InsertAsync(blog, token: default);

        // Assert
        var savedBlog = await BlogEfRepo.GetWithNavigationPropertiesAsync(
            blog.Id,
            [(Expression<Func<Blog, dynamic>>)(b => b.Tags)],
            token: default);

        Assert.IsNotNull(savedBlog);
        Assert.AreEqual(2, savedBlog.Tags.Count);
        Assert.IsTrue(savedBlog.Tags.Any(t => t.Name == "技术"));
        Assert.IsTrue(savedBlog.Tags.Any(t => t.Name == "编程"));
    }

    #endregion

    #region Update Tests

    /// <summary>
    /// 测试更新博客基本信息
    /// </summary>
    [TestMethod]
    public async Task UpdateBlog_BasicInfo_ShouldUpdateSuccessfully()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            Author = "原作者",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog, token: default);

        // Act
        blog.Title = "更新后的标题";
        blog.Content = "更新后的内容";
        await BlogEfRepo.UpdateAsync(blog, token: default);

        // Assert
        var updatedBlog = await BlogEfRepo.GetAsync(blog.Id, token: default);
        Assert.IsNotNull(updatedBlog);
        Assert.AreEqual("更新后的标题", updatedBlog.Title);
        Assert.AreEqual("更新后的内容", updatedBlog.Content);
        Assert.IsTrue(updatedBlog.UpdatedAt.HasValue);
        Assert.AreEqual(1, updatedBlog.UpdatedBy);
    }

    /// <summary>
    /// 测试更新博客标签
    /// </summary>
    [TestMethod]
    public async Task UpdateBlog_Tags_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tags = new List<BlogTag>
        {
            new() { Name = "技术", Description = "技术相关文章" },
            new() { Name = "编程", Description = "编程技巧" }
        };
        await BlogTagEfRepo.InsertAsync(tags, token: default);

        var blog = new Blog
        {
            Title = "测试博客",
            Content = "这是一篇测试博客",
            Author = "测试作者",
            PublishTime = DateTime.Now,
            Tags = new List<BlogTag> { tags[0] }
        };
        await BlogEfRepo.InsertAsync(blog, token: default);

        // Act
        blog.Tags.Add(tags[1]);
        await BlogEfRepo.UpdateAsync(blog, token: default);

        // Assert
        var updatedBlog = await BlogEfRepo.GetWithNavigationPropertiesAsync(
            blog.Id,
            [(Expression<Func<Blog, dynamic>>)(b => b.Tags)],
            token: default);

        Assert.IsNotNull(updatedBlog);
        Assert.AreEqual(2, updatedBlog.Tags.Count);
        Assert.IsTrue(updatedBlog.Tags.Any(t => t.Name == "技术"));
        Assert.IsTrue(updatedBlog.Tags.Any(t => t.Name == "编程"));
    }

    #endregion

    #region Delete Tests

    /// <summary>
    /// 测试软删除博客
    /// </summary>
    [TestMethod]
    public async Task SoftDeleteBlog_ShouldSetDeleteFields()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "测试博客",
            Content = "这是一篇测试博客",
            Author = "测试作者",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog, token: default);

        // Act
        blog.IsDeleted = true;
        await BlogEfRepo.UpdateAsync(blog, token: default);

        // Assert
        var deletedBlog = await DbContext.Blogs.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == blog.Id);
        Assert.IsNotNull(deletedBlog);
        Assert.IsTrue(deletedBlog.IsDeleted);
        Assert.IsTrue(deletedBlog.DeletedAt.HasValue);
        Assert.AreEqual(1, deletedBlog.DeletedBy);

        // 验证正常查询无法获取已删除的博客
        var notFound = await BlogEfRepo.GetAsync(blog.Id, token: default);
        Assert.IsNull(notFound);
    }

    #endregion

    #region Query Tests

    /// <summary>
    /// 测试根据作者查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByAuthor_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "博客1", Content = "内容1", Author = "张三", PublishTime = DateTime.Now },
            new() { Title = "博客2", Content = "内容2", Author = "李四", PublishTime = DateTime.Now },
            new() { Title = "博客3", Content = "内容3", Author = "张三", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs, token: default);

        // Act
        var result = await BlogEfRepo.GetListAsync(b => b.Author == "张三", token: default);
        var authorBlogs = result.ToList();

        // Assert
        Assert.AreEqual(2, authorBlogs.Count);
        Assert.IsTrue(authorBlogs.All(b => b.Author == "张三"));
    }

    /// <summary>
    /// 测试根据标签查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByTag_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var tag = new BlogTag { Name = "技术", Description = "技术相关文章" };
        await BlogTagEfRepo.InsertAsync(tag, token: default);

        var blogs = new List<Blog>
        {
            new() { Title = "博客1", Content = "内容1", Author = "作者1", PublishTime = DateTime.Now, Tags = new List<BlogTag> { tag } },
            new() { Title = "博客2", Content = "内容2", Author = "作者2", PublishTime = DateTime.Now },
            new() { Title = "博客3", Content = "内容3", Author = "作者3", PublishTime = DateTime.Now, Tags = new List<BlogTag> { tag } }
        };
        await BlogEfRepo.InsertAsync(blogs, token: default);

        // Act
        var result = await BlogEfRepo.GetListAsync(
            whereExpression: b => b.Tags.Any(t => t.Name == "技术"),
            noTracking: true,
            token: default);
        var taggedBlogs = result.ToList();

        // Assert
        Assert.AreEqual(2, taggedBlogs.Count);
        
        // 验证每个博客的标签
        foreach (var blog in taggedBlogs)
        {
            var blogWithTags = await BlogEfRepo.GetWithNavigationPropertiesAsync(
                blog.Id,
                [(Expression<Func<Blog, dynamic>>)(b => b.Tags)],
                token: default);
            Assert.IsTrue(blogWithTags!.Tags.Any(t => t.Name == "技术"));
        }
    }

    /// <summary>
    /// 测试分页查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByPage_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var blogs = new List<Blog>();
        for (int i = 1; i <= 5; i++)
        {
            blogs.Add(new Blog
            {
                Title = $"博客{i}",
                Content = $"内容{i}",
                Author = $"作者{i}",
                PublishTime = DateTime.Now.AddDays(-i)
            });
        }
        await BlogEfRepo.InsertAsync(blogs, token: default);

        var pageSize = 2;
        var skipCount = 1;

        // Act
        var result = await BlogEfRepo.GetListAsync(
            whereExpression: null,
            noTracking: true,
            token: default);

        var pagedBlogs = result
            .OrderByDescending(b => b.PublishTime)
            .Skip(skipCount)
            .Take(pageSize)
            .ToList();

        // Assert
        Assert.AreEqual(pageSize, pagedBlogs.Count);
        Assert.IsTrue(pagedBlogs[0].PublishTime > pagedBlogs[1].PublishTime);
    }

    #endregion
}