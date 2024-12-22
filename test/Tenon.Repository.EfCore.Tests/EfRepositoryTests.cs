using System.Linq.Expressions;
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
    public override async Task Setup()
    {
        await base.Setup();
    }

    /// <summary>
    /// 每个测试方法执行后的清理
    /// </summary>
    [TestCleanup]
    public override async Task Cleanup()
    {
        await base.Cleanup();
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
            Author = "测试作者",
            PublishTime = DateTime.Now
        };

        // Act
        var result = await BlogEfRepo.InsertAsync(blog);

        // Assert
        Assert.IsTrue(result > 0);
        Assert.IsTrue(blog.Id > 0);
        var savedBlog = await BlogEfRepo.GetAsync(blog.Id, false);
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
            new() { Title = "博客1", Content = "内容1", Author = "作者1", PublishTime = DateTime.Now },
            new() { Title = "博客2", Content = "内容2", Author = "作者2", PublishTime = DateTime.Now }
        };

        // Act
        var result = await BlogEfRepo.InsertAsync(blogs);

        // Assert
        Assert.IsTrue(result > 0);
        var savedBlogs = await BlogEfRepo.GetListAsync(null, default, false);
        Assert.AreEqual(blogs.Count, savedBlogs.Count());
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
            Author = "原始作者",
            PublishTime = DateTime.Now.AddDays(-1)
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        blog.Title = "更新后的标题";
        blog.PublishTime = DateTime.Now;
        var result = await BlogEfRepo.UpdateAsync(blog);

        // Assert
        Assert.IsTrue(result > 0);
        var updatedBlog = await BlogEfRepo.GetAsync(blog.Id, false);
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
            Author = "待删除的作者",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        var result = await BlogEfRepo.RemoveAsync(blog);

        // Assert
        Assert.IsTrue(result > 0);
        var deletedBlog = await BlogEfRepo.GetAsync(blog.Id, false);
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
            Author = "测试作者",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        var result = await BlogEfRepo.GetAsync(blog.Id, false);

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
                Author = $"作者{i}",
                PublishTime = DateTime.Now.AddDays(-i)
            });
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        Expression<Func<Blog, dynamic>>? orderByExpression = b => b.PublishTime;
        var pageResult = await BlogEfRepo.GetPagedListAsync(
            2,
            5,
            null,
            orderByExpression,
            false,
            true
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
            new() { Title = "技术博客", Content = "技术内容", Author = "技术作者", PublishTime = DateTime.Now },
            new() { Title = "生活随笔", Content = "生活内容", Author = "生活作者", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var results = await BlogEfRepo.GetListAsync(b => b.Title.Contains("技术"), default, false);

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
            Author = "测试作者",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // Act
        var exists = await BlogEfRepo.AnyAsync(b => b.Title == "唯一标题");

        // Assert
        Assert.IsTrue(exists);
    }

    /// <summary>
    /// 测试统计查询
    /// </summary>
    [TestMethod]
    public async Task CountAsync_WithValidCondition_ShouldReturnCorrectCount()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "技术博客1", Content = "技术内容", Author = "技术作者", PublishTime = DateTime.Now },
            new() { Title = "技术博客2", Content = "技术内容", Author = "技术作者", PublishTime = DateTime.Now },
            new() { Title = "生活随笔", Content = "生活内容", Author = "生活作者", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var count = await BlogEfRepo.CountAsync(b => b.Title.StartsWith("技术"));

        // Assert
        Assert.AreEqual(2, count);
    }

    /// <summary>
    /// 测试批量更新实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithMultipleEntities_ShouldUpdateAllSuccessfully()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "原始博客1", Content = "内容1", Author = "作者1", PublishTime = DateTime.Now },
            new() { Title = "原始博客2", Content = "内容2", Author = "作者2", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        foreach (var blog in blogs)
        {
            blog.Title = $"更新后的{blog.Title}";
        }
        var result = await BlogEfRepo.UpdateAsync(blogs);

        // Assert
        Assert.IsTrue(result > 0);
        var updatedBlogs = await BlogEfRepo.GetListAsync(null, default, false);
        Assert.AreEqual(blogs.Count, updatedBlogs.Count());
        foreach (var blog in updatedBlogs)
        {
            Assert.IsTrue(blog.Title.StartsWith("更新后的"));
        }
    }

    /// <summary>
    /// 测试复杂条件查询
    /// </summary>
    [TestMethod]
    public async Task GetListAsync_WithComplexConditions_ShouldReturnCorrectResults()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "技术博客", Content = "重要内容", Author = "技术作者", PublishTime = DateTime.Today },
            new() { Title = "技术分享", Content = "普通内容", Author = "技术作者", PublishTime = DateTime.Today.AddDays(-1) },
            new() { Title = "生活随笔", Content = "重要内容", Author = "生活作者", PublishTime = DateTime.Today.AddDays(-2) }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var results = await BlogEfRepo.GetListAsync(
            b => b.Title.Contains("技术") && 
                 b.Content.Contains("重要") && 
                 b.PublishTime >= DateTime.Today.AddDays(-1),
            default,
            false
        );

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Count());
        Assert.IsTrue(results.First().Title.Contains("技术") && results.First().Content.Contains("重要"));
    }
}