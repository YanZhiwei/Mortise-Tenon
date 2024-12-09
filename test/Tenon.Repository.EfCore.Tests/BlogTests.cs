using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

[TestClass]
public class BlogTests : TestBase
{
    /// <summary>
    /// 测试根据作者查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByAuthor_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var author = "张三";

        // Act
        var blogs = await BlogEfRepo.GetListAsync(b => b.Author == author, token: default);
        var blogList = blogs.ToList();

        // Assert
        Assert.AreEqual(2, blogList.Count);
        Assert.IsTrue(blogList.All(b => b.Author == author));
    }

    /// <summary>
    /// 测试根据标签查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByTag_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var tagName = "技术";

        // 先获取包含指定标签的博客ID
        var blogs = await BlogEfRepo.GetListAsync(
            whereExpression: b => b.Tags.Any(t => t.Name == tagName),
            noTracking: true,
            token: default);

        // 获取博客详情（包含标签）
        var blogsWithTags = new List<Blog>();
        foreach (var blog in blogs)
        {
            var blogWithTags = await BlogEfRepo.GetWithNavigationPropertiesAsync(
                blog.Id,
                [(Expression<Func<Blog, dynamic>>)(b => b.Tags)],
                token: default);

            if (blogWithTags != null)
            {
                blogsWithTags.Add(blogWithTags);
            }
        }

        var orderedBlogs = blogsWithTags.OrderBy(b => b.Id).ToList();

        // Assert
        Assert.AreEqual(2, orderedBlogs.Count);
        Assert.IsTrue(orderedBlogs.All(b => b.Tags.Any(t => t.Name == tagName)));

        var blogTitles = orderedBlogs.Select(b => b.Title).ToList();
        CollectionAssert.Contains(blogTitles, "第一篇博客");
        CollectionAssert.Contains(blogTitles, "第三篇博客");
    }

    /// <summary>
    /// 测试创建带标签的博客
    /// </summary>
    [TestMethod]
    public async Task CreateBlogWithTags_ShouldCreateSuccessfully()
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
        Assert.AreEqual(1, savedBlog.CreatedBy);
        Assert.IsTrue(savedBlog.CreatedAt > DateTimeOffset.MinValue);
        Assert.IsFalse(savedBlog.IsDeleted);
        Assert.IsNull(savedBlog.UpdatedAt);
        Assert.IsNull(savedBlog.UpdatedBy);
        Assert.IsNull(savedBlog.DeletedAt);
        Assert.IsNull(savedBlog.DeletedBy);
    }

    /// <summary>
    /// 测试更新博客
    /// </summary>
    [TestMethod]
    public async Task UpdateBlog_ShouldUpdateAuditFields()
    {
        // Arrange
        var blog = await BlogEfRepo.GetAsync(1, noTracking: false, token: default);
        Assert.IsNotNull(blog);

        var originalCreatedAt = blog.CreatedAt;
        var originalCreatedBy = blog.CreatedBy;

        // Act
        blog.Title = "更新后的标题";
        await BlogEfRepo.UpdateAsync(blog, token: default);

        // Assert
        var updatedBlog = await BlogEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(updatedBlog);
        Assert.AreEqual("更新后的标题", updatedBlog.Title);
        Assert.AreEqual(originalCreatedAt, updatedBlog.CreatedAt);
        Assert.AreEqual(originalCreatedBy, updatedBlog.CreatedBy);
        Assert.IsTrue(updatedBlog.UpdatedAt.HasValue);
        Assert.AreEqual(1, updatedBlog.UpdatedBy);
        Assert.IsFalse(updatedBlog.IsDeleted);
        Assert.IsNull(updatedBlog.DeletedAt);
        Assert.IsNull(updatedBlog.DeletedBy);
    }

    /// <summary>
    /// 测试软删除博客
    /// </summary>
    [TestMethod]
    public async Task SoftDeleteBlog_ShouldSetDeleteFields()
    {
        // Arrange
        var blog = await BlogEfRepo.GetAsync(1, noTracking: false, token: default);
        Assert.IsNotNull(blog);

        var originalCreatedAt = blog.CreatedAt;
        var originalCreatedBy = blog.CreatedBy;

        // Act
        blog.IsDeleted = true;
        await BlogEfRepo.UpdateAsync(blog, token: default);

        // Assert
        var deletedBlog = await DbContext.Blogs.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == 1);
        Assert.IsNotNull(deletedBlog);
        Assert.IsTrue(deletedBlog.IsDeleted);
        Assert.AreEqual(originalCreatedAt, deletedBlog.CreatedAt);
        Assert.AreEqual(originalCreatedBy, deletedBlog.CreatedBy);
        Assert.IsTrue(deletedBlog.DeletedAt.HasValue);
        Assert.AreEqual(1, deletedBlog.DeletedBy);
    }

    /// <summary>
    /// 测试删除博客（级联删除评论）
    /// </summary>
    [TestMethod]
    public async Task DeleteBlog_ShouldDeleteBlogAndComments()
    {
        // Arrange
        var blogId = 1L;

        // 获取博客相关的评论数
        var commentCount = await BlogCommentEfRepo.CountAsync(c => c.BlogId == blogId, token: default);
        Assert.AreNotEqual(0, commentCount);

        // Act
        var blog = await DbContext.Blogs.FindAsync(blogId);
        Assert.IsNotNull(blog);
        DbContext.Blogs.Remove(blog);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedBlog = await BlogEfRepo.GetAsync(blogId, noTracking: true, token: default);
        Assert.IsNull(deletedBlog);

        // 验证评论也被删除
        var remainingComments = await BlogCommentEfRepo.CountAsync(c => c.BlogId == blogId, token: default);
        Assert.AreEqual(0, remainingComments);
    }

    /// <summary>
    /// 测试获取博客详情（包含标签和评论）
    /// </summary>
    [TestMethod]
    public async Task GetBlogDetail_ShouldIncludeTagsAndComments()
    {
        // Arrange
        var blogId = 1L;

        // Act
        var blog = await BlogEfRepo.GetWithNavigationPropertiesAsync(
            blogId,
            new[] 
            { 
                (Expression<Func<Blog, dynamic>>)(b => b.Tags),
                b => b.Comments
            },
            token: default);

        // Assert
        Assert.IsNotNull(blog, "博客不应为空");
        Assert.IsTrue(blog.Tags.Any(), "博客应该有标签");
        Assert.IsTrue(blog.Comments.Any(), "博客应该有评论");

        // 验证标签
        foreach (var tag in blog.Tags)
        {
            Assert.IsFalse(string.IsNullOrEmpty(tag.Name), "标签名称不应为空");
        }

        // 验证评论
        foreach (var comment in blog.Comments)
        {
            Assert.IsFalse(string.IsNullOrEmpty(comment.Content), "评论内容不应为空");
            Assert.IsFalse(string.IsNullOrEmpty(comment.Commenter), "评论者不应为空");
            Assert.AreNotEqual(default(DateTime), comment.CommentTime, "评论时间不应为默认值");

            if (comment.Children.Any())
            {
                foreach (var childComment in comment.Children)
                {
                    Assert.AreEqual(comment.Id, childComment.ParentId);
                    Assert.IsFalse(string.IsNullOrEmpty(childComment.Content));
                }
            }
        }

        var totalComments = blog.Comments.Count + blog.Comments.Sum(c => c.Children.Count);
        Assert.IsTrue(totalComments > 0, "博客应该至少有一条评论");
    }

    /// <summary>
    /// 测试按阅读量排序查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByReadCount_ShouldReturnOrderedBlogs()
    {
        // Act
        var blogs = await BlogEfRepo.GetListAsync(
            whereExpression: null,
            noTracking: true,
            token: default);

        var blogList = blogs.OrderByDescending(b => b.ReadCount).ToList();

        // Assert
        Assert.IsTrue(blogList.Count > 0);
        for (int i = 1; i < blogList.Count; i++)
        {
            Assert.IsTrue(blogList[i - 1].ReadCount >= blogList[i].ReadCount);
        }
    }

    /// <summary>
    /// 测试分页查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByPage_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var pageSize = 2;
        var skipCount = 1;

        // Act
        var blogs = await BlogEfRepo.GetListAsync(
            whereExpression: null,
            noTracking: true,
            token: default);

        var pagedBlogs = blogs
            .OrderByDescending(b => b.PublishTime)
            .Skip(skipCount)
            .Take(pageSize)
            .ToList();

        // Assert
        Assert.AreEqual(pageSize, pagedBlogs.Count);
        Assert.IsTrue(pagedBlogs[0].PublishTime > pagedBlogs[1].PublishTime);
    }
}