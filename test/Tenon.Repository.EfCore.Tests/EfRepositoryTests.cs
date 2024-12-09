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
        // 直接清空数据库
        DbContext.Set<Blog>().RemoveRange(DbContext.Set<Blog>());
        try
        {
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // 忽略并重置上下文
            DbContext.ChangeTracker.Clear();
        }
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

    /// <summary>
    /// 测试批量更新实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithMultipleEntities_ShouldUpdateAllSuccessfully()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "原始博客1", Content = "内容1", PublishTime = DateTime.Now },
            new() { Title = "原始博客2", Content = "内容2", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // 修改标题
        foreach (var blog in blogs) blog.Title = $"更新后的{blog.Title}";

        // Act
        var result = await BlogEfRepo.UpdateAsync(blogs);

        // Assert
        Assert.IsTrue(result > 0);
        var updatedBlogs = await DbContext.Blogs.ToListAsync();
        Assert.AreEqual(blogs.Count, updatedBlogs.Count);
        foreach (var blog in updatedBlogs) Assert.IsTrue(blog.Title.StartsWith("更新后的"));
    }

    /// <summary>
    /// 测试批量删除实体
    /// </summary>
    [TestMethod]
    public async Task RemoveAsync_WithMultipleEntities_ShouldRemoveAllSuccessfully()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "待删除博客1", Content = "内容1", PublishTime = DateTime.Now },
            new() { Title = "待删除博客2", Content = "内容2", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var result = await BlogEfRepo.RemoveAsync(blogs);

        // Assert
        Assert.IsTrue(result > 0);
        var remainingBlogs = await DbContext.Blogs.ToListAsync();
        Assert.AreEqual(0, remainingBlogs.Count);
    }

    /// <summary>
    /// 测试排序和查询组合
    /// </summary>
    [TestMethod]
    public async Task GetListAsync_WithOrderByAndFilter_ShouldReturnOrderedFilteredResults()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "技术博客", Content = "内容A", PublishTime = DateTime.Now.AddDays(-2) },
            new() { Title = "技术分享", Content = "内容B", PublishTime = DateTime.Now.AddDays(-1) },
            new() { Title = "生活随笔", Content = "内容C", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var results = (await BlogEfRepo.GetListAsync(
            b => b.Title.Contains("技术"),
            default
        )).OrderByDescending(b => b.PublishTime);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(2, results.Count());
        Assert.AreEqual("技术分享", results.First().Title);
    }

    /// <summary>
    /// 测试获取不存在的实体
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        const long invalidId = 99999;

        // Act
        var result = await BlogEfRepo.GetAsync(invalidId, default);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// 测试空集合的分页查询
    /// </summary>
    [TestMethod]
    public async Task GetPagedListAsync_WithEmptyData_ShouldReturnEmptyPage()
    {
        // Act
        var pageResult = await BlogEfRepo.GetPagedListAsync(1, 10);

        // Assert
        Assert.IsNotNull(pageResult);
        Assert.AreEqual(0, pageResult.TotalCount);
        Assert.AreEqual(0, pageResult.Items.Count());
        Assert.AreEqual(1, pageResult.PageIndex);
        Assert.AreEqual(10, pageResult.PageSize);
    }

    /// <summary>
    /// 测试更新不存在的实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithNonExistentEntity_ShouldThrowException()
    {
        // Arrange
        var nonExistentBlog = new Blog
        {
            Id = 99999,
            Title = "不存在的博客",
            Content = "测试内容",
            PublishTime = DateTime.Now
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await BlogEfRepo.UpdateAsync(nonExistentBlog);
        });
    }

    /// <summary>
    /// 测试删除不存在的实体
    /// </summary>
    [TestMethod]
    public async Task RemoveAsync_WithNonExistentEntity_ShouldThrowException()
    {
        // Arrange
        var nonExistentBlog = new Blog
        {
            Id = 99999,
            Title = "不存在的博客",
            Content = "测试内容",
            PublishTime = DateTime.Now
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<DbUpdateConcurrencyException>(async () =>
        {
            await BlogEfRepo.RemoveAsync(nonExistentBlog);
        });
    }

    /// <summary>
    /// 测试分页查询 - 页码超出范围
    /// </summary>
    [TestMethod]
    public async Task GetPagedListAsync_WithPageIndexOutOfRange_ShouldReturnEmptyList()
    {
        // Arrange
        var blogs = new List<Blog>();
        for (var i = 1; i <= 5; i++)
            blogs.Add(new Blog
            {
                Title = $"博客{i}",
                Content = $"内容{i}",
                PublishTime = DateTime.Now
            });
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var pageResult = await BlogEfRepo.GetPagedListAsync(99, 10);

        // Assert
        Assert.IsNotNull(pageResult);
        Assert.AreEqual(5, pageResult.TotalCount);
        Assert.AreEqual(0, pageResult.Items.Count());
        Assert.AreEqual(99, pageResult.PageIndex);
    }

    /// <summary>
    /// 测试复杂条件查询
    /// </summary>
    [TestMethod]
    public async Task GetListAsync_WithComplexConditions_ShouldReturnCorrectResults()
    {
        // Arrange
        var baseTime = DateTime.Now.Date; // 使用日期，去除时间部分
        var blogs = new List<Blog>
        {
            new()
            {
                Title = "技术博客",
                Content = "重要内容",
                PublishTime = baseTime.AddDays(-1)
            },
            new()
            {
                Title = "技术分享",
                Content = "普通内容",
                PublishTime = baseTime.AddDays(-2)
            },
            new()
            {
                Title = "生活随笔",
                Content = "重要内容",
                PublishTime = baseTime.AddDays(-3)
            }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var results = await BlogEfRepo.GetListAsync(
            b => b.Title.Contains("技术") && b.Content.Contains("重要") // 修改条件逻辑
                                        && b.PublishTime >= baseTime.AddDays(-2),
            default
        );

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Count());
        Assert.AreEqual("技术博客", results.First().Title);
    }

    /// <summary>
    /// 测试批量插入空集合
    /// </summary>
    [TestMethod]
    public async Task InsertAsync_WithEmptyList_ShouldReturnZero()
    {
        // Arrange
        var emptyList = new List<Blog>();

        // Act
        var result = await BlogEfRepo.InsertAsync(emptyList);

        // Assert
        Assert.AreEqual(0, result);
        var count = await DbContext.Blogs.CountAsync();
        Assert.AreEqual(0, count);
    }

    /// <summary>
    /// 测试查询结果为空的条件查询
    /// </summary>
    [TestMethod]
    public async Task GetListAsync_WithNoMatchingCondition_ShouldReturnEmptyList()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "技术博客", Content = "内容A", PublishTime = DateTime.Now },
            new() { Title = "生活随笔", Content = "内容B", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        // Act
        var results = await BlogEfRepo.GetListAsync(
            b => b.Title.Contains("不存在的标题"),
            default
        );

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count());
    }

    /// <summary>
    /// 测试更新未跟踪的实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithUnTrackedEntity_ShouldThrowException()
    {
        // Arrange
        var blog = new Blog
        {
            Id = 1,
            Title = "未跟踪的博客",
            Content = "测试内容",
            PublishTime = DateTime.Now
        };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await BlogEfRepo.UpdateAsync(blog);
        });
    }

    /// <summary>
    /// 测试批量更新部分存在的实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithPartialExistingEntities_ShouldUpdateExistingOnes()
    {
        // Arrange
        var blogs = new List<Blog>
        {
            new() { Title = "原始博客1", Content = "内容1", PublishTime = DateTime.Now },
            new() { Title = "原始博客2", Content = "内容2", PublishTime = DateTime.Now }
        };
        await BlogEfRepo.InsertAsync(blogs);

        var existingBlog = blogs[0];
        existingBlog.Title = "更新后的博客1";

        var trackedEntities = new List<Blog> { existingBlog };
        await BlogEfRepo.UpdateAsync(trackedEntities);

        // Act
        var updatedBlog = await BlogEfRepo.GetAsync(existingBlog.Id, default);

        // Assert
        Assert.IsNotNull(updatedBlog);
        Assert.AreEqual("更新后的博客1", updatedBlog.Title);
    }

    /// <summary>
    /// 测试更新实体的所有属性
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithAllPropertiesChanged_ShouldUpdateAllProperties()
    {
        // Arrange
        var originalBlog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            PublishTime = DateTime.Now.AddDays(-1)
        };
        await BlogEfRepo.InsertAsync(originalBlog);

        var updatedTime = DateTime.Now;
        originalBlog.Title = "新标题";
        originalBlog.Content = "新内容";
        originalBlog.PublishTime = updatedTime;

        // Act
        await BlogEfRepo.UpdateAsync(originalBlog);
        var result = await BlogEfRepo.GetAsync(originalBlog.Id, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("新标题", result.Title);
        Assert.AreEqual("新内容", result.Content);
        Assert.AreEqual(updatedTime.Date, result.PublishTime.Date);
    }

    /// <summary>
    /// 测试并发更新同一实体
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithConcurrentUpdates_ShouldHandleLastWriteWins()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        // 第一次更新
        blog.Title = "第一次更新";
        await BlogEfRepo.UpdateAsync(blog);

        // 第二次更新
        blog.Title = "第二次更新";
        await BlogEfRepo.UpdateAsync(blog);

        // Act
        var result = await BlogEfRepo.GetAsync(blog.Id, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("第二次更新", result.Title);
    }

    /// <summary>
    /// 测试更新指定属性
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithSpecificProperties_ShouldOnlyUpdateSpecifiedOnes()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            PublishTime = DateTime.Now.AddDays(-1)
        };
        await BlogEfRepo.InsertAsync(blog);

        var originalPublishTime = blog.PublishTime;
        blog.Title = "新标题";
        blog.Content = "新内容";
        blog.PublishTime = DateTime.Now;

        // Act
        await BlogEfRepo.UpdateAsync(blog,
        [
            b => b.Title,
                b => b.Content
        ]);

        var result = await BlogEfRepo.GetAsync(blog.Id, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("新标题", result.Title);
        Assert.AreEqual("新内容", result.Content);
        Assert.AreEqual(originalPublishTime, result.PublishTime); // PublishTime 不应该被更新
    }

    /// <summary>
    /// 测试使用空属性表达式数组更新 - 应该更新所有属性
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithEmptyUpdateExpressions_ShouldUpdateAllProperties()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        var newTitle = "新标题";
        var newContent = "新内容";
        var newPublishTime = DateTime.Now.AddDays(1);

        blog.Title = newTitle;
        blog.Content = newContent;
        blog.PublishTime = newPublishTime;

        // Act
        await BlogEfRepo.UpdateAsync(blog, []);

        var result = await BlogEfRepo.GetAsync(blog.Id, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(newTitle, result.Title);
        Assert.AreEqual(newContent, result.Content);
        Assert.AreEqual(newPublishTime.Date, result.PublishTime.Date);
    }

    /// <summary>
    /// 测试更新单个属性
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithSingleProperty_ShouldOnlyUpdateThatProperty()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "原始标题",
            Content = "原始内容",
            PublishTime = DateTime.Now
        };
        await BlogEfRepo.InsertAsync(blog);

        var originalContent = blog.Content;
        var originalPublishTime = blog.PublishTime;

        blog.Title = "新标题";
        blog.Content = "新内容";
        blog.PublishTime = DateTime.Now.AddDays(1);

        // Act
        await BlogEfRepo.UpdateAsync(blog,
        [
            b => b.Title
        ]);

        var result = await BlogEfRepo.GetAsync(blog.Id, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("新标题", result.Title);
        Assert.AreEqual(originalContent, result.Content);
        Assert.AreEqual(originalPublishTime, result.PublishTime);
    }
}