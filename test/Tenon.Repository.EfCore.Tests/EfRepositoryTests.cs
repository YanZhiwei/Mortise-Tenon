using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

/// <summary>
/// EfRepository 仓储测试类
/// </summary>
[TestClass]
public class EfRepositoryTests : TestBase
{
    /// <summary>
    /// 每个测试方法执行前的初始化
    /// </summary>
    [TestInitialize]
    public async Task TestInitialize()
    {
        await CleanupAsync();
    }

    /// <summary>
    /// 每个测试方法执行后的清理
    /// </summary>
    [TestCleanup]
    public async Task CleanupAsync()
    {
        var entities = await DbContext.Set<Blog>().ToListAsync();
        DbContext.RemoveRange(entities);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 测试插入单个实体
    /// </summary>
    [TestMethod]
    public async Task InsertAsync_WithValidEntity_ShouldInsertSuccessfully()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "测试博客标题",
            Content = "测试博客内容",
            PublishTime = DateTime.Now
        };

        // Act
        var result = await BlogEfRepo.InsertAsync(blog);

        // Assert
        Assert.IsTrue(result > 0);
        Assert.IsTrue(blog.Id > 0);
        var savedBlog = await DbContext.Blogs.FindAsync(blog.Id);
        Assert.IsNotNull(savedBlog);
        Assert.AreEqual(blog.Title, savedBlog.Title);
    }

    /// <summary>
    /// 测试批量插入实体
    /// </summary>
    [TestMethod]
    public async Task InsertAsync_WithMultipleEntities_ShouldInsertAllSuccessfully()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "博客1", Content = "内容1", PublishTime = DateTime.Now },
            new() { Title = "博客2", Content = "内容2", PublishTime = DateTime.Now }
        };

        // Act
        var result = await BlogEfRepo.InsertAsync(blogs);

        // Assert
        Assert.IsTrue(result > 0);
        var savedBlogs = await DbContext.Blogs.ToListAsync();
        Assert.AreEqual(blogs.Count, savedBlogs.Count);
    }

    /// <summary>
    /// 测试更新实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateSuccessfully()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        blog.Title = "更新后的标题";
        var result = await BlogEfRepo.UpdateAsync(blog);

        // Assert
        Assert.IsTrue(result > 0);
        var updatedBlog = await DbContext.Blogs.FindAsync(blog.Id);
        Assert.IsNotNull(updatedBlog);
        Assert.AreEqual("更新后的标题", updatedBlog.Title);
    }

    /// <summary>
    /// 测试删除实体
    /// </summary>
    [TestMethod]
    public async Task RemoveAsync_WithValidEntity_ShouldRemoveSuccessfully()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "待删除的博客",
            Content = "待删除的内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        var result = await BlogEfRepo.RemoveAsync(blog);

        // Assert
        Assert.IsTrue(result > 0);
        var deletedBlog = await DbContext.Blogs.FindAsync(blog.Id);
        Assert.IsNull(deletedBlog);
    }

    /// <summary>
    /// 测试根据ID获取实体
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "测试博客",
            Content = "测试内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        var result = await BlogEfRepo.GetAsync(blog.Id, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(blog.Title, result.Title);
    }

    /// <summary>
    /// 测试分页查询
    /// </summary>
    [TestMethod]
    public async Task GetPagedListAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var blogs = new List<Blog>();
        for (var i = 1; i <= 20; i++)
            blogs.Add(new Blog
            {
                Title = $"博客{i}",
                Content = $"内容{i}",
                PublishTime = DateTime.Now
            });
        await BlogEfRepo.InsertAsync(blogs);

        // 确保数据已经正确插入
        var totalCount = await DbContext.Set<Blog>().CountAsync();
        Assert.AreEqual(20, totalCount, "数据插入验证失败");

        // Act
        var pageResult = await BlogEfRepo.GetPagedListAsync(
            2,
            5
        );

        // Assert
        Assert.IsNotNull(pageResult);
        Assert.AreEqual(20, pageResult.TotalCount, "总记录数不匹配");
        Assert.AreEqual(5, pageResult.Items.Count(), "分页大小不匹配");
        Assert.AreEqual(2, pageResult.PageIndex, "页码不匹配");
        Assert.AreEqual(5, pageResult.PageSize, "每页大小不匹配");
    }

    /// <summary>
    /// 测试条件查询
    /// </summary>
    [TestMethod]
    public async Task GetListAsync_WithWhereExpression_ShouldReturnFilteredResults()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "技术博客", Content = "技术内容", PublishTime = DateTime.Now },
            new() { Title = "生活随笔", Content = "生活内容", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var results = await BlogEfRepo.GetListAsync(b => b.Title.Contains("技术"), default);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Count());
        Assert.IsTrue(results.First().Title.Contains("技术"));
    }

    /// <summary>
    /// 测试检查实体是否存在
    /// </summary>
    [TestMethod]
    public async Task AnyAsync_WithExistingCondition_ShouldReturnTrue()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "唯一标题",
            Content = "测试内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        var exists = await BlogEfRepo.AnyAsync(b => b.Title == "唯一标题");

        // Assert
        Assert.IsTrue(exists);
    }

    /// <summary>
    /// 测试统计符合条件的实体数量
    /// </summary>
    [TestMethod]
    public async Task CountAsync_WithValidCondition_ShouldReturnCorrectCount()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "技术博客1", Content = "技术内容", PublishTime = DateTime.Now },
            new() { Title = "技术博客2", Content = "技术内容", PublishTime = DateTime.Now },
            new() { Title = "生活随笔", Content = "生活内容", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var count = await BlogEfRepo.CountAsync(b => b.Title.StartsWith("技术"));

        // Assert
        Assert.AreEqual(2, count);
    }
}