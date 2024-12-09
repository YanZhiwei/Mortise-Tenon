using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

[TestClass]
public class BlogCommentTests : TestBase
{
    /// <summary>
    /// 测试创建评论
    /// </summary>
    [TestMethod]
    public async Task CreateComment_ShouldCreateSuccessfully()
    {
        // Arrange
        var comment = new BlogComment
        {
            BlogId = 1,
            Content = "这是一条测试评论",
            Commenter = "测试用户",
            CommentTime = DateTime.Now
        };

        // Act
        await BlogCommentEfRepo.InsertAsync(comment, token: default);

        // Assert
        var savedComment = await BlogCommentEfRepo.GetAsync(comment.Id, token: default);
        Assert.IsNotNull(savedComment);
        Assert.AreEqual(comment.Content, savedComment.Content);
        Assert.AreEqual(1, savedComment.CreatedBy);
        Assert.IsTrue(savedComment.CreatedAt > DateTimeOffset.MinValue);
        Assert.IsFalse(savedComment.IsDeleted);
    }

    /// <summary>
    /// 测试获取博客的所有评论
    /// </summary>
    [TestMethod]
    public async Task GetBlogComments_ShouldReturnCorrectComments()
    {
        // Arrange
        var blogId = 1L;

        // Act
        var comments = await DbContext.BlogComments
            .Include(c => c.Children)
            .Where(c => c.BlogId == blogId)
            .ToListAsync();

        // Assert
        Assert.IsTrue(comments.Any());
        foreach (var comment in comments)
        {
            Assert.AreEqual(blogId, comment.BlogId);
            Assert.IsFalse(string.IsNullOrEmpty(comment.Content));
            Assert.IsFalse(string.IsNullOrEmpty(comment.Commenter));
        }
    }

    /// <summary>
    /// 测试获取评论及其子评论
    /// </summary>
    [TestMethod]
    public async Task GetCommentWithReplies_ShouldReturnCorrectData()
    {
        // Arrange
        var parentCommentId = 1L;

        // Act
        var comment = await DbContext.BlogComments
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == parentCommentId);

        // Assert
        Assert.IsNotNull(comment);
        Assert.IsTrue(comment.Children.Any());
        Assert.IsFalse(string.IsNullOrEmpty(comment.Content));
        
        foreach (var reply in comment.Children)
        {
            Assert.AreEqual(comment.Id, reply.ParentId);
            Assert.IsFalse(string.IsNullOrEmpty(reply.Content));
            Assert.IsFalse(string.IsNullOrEmpty(reply.Commenter));
        }
    }

    /// <summary>
    /// 测试更新评论
    /// </summary>
    [TestMethod]
    public async Task UpdateComment_ShouldUpdateAuditFields()
    {
        // Arrange
        var comment = await BlogCommentEfRepo.GetAsync(1, noTracking: false, token: default);
        Assert.IsNotNull(comment);

        var originalCreatedAt = comment.CreatedAt;
        var originalCreatedBy = comment.CreatedBy;

        // Act
        comment.Content = "更新后的评论内容";
        await BlogCommentEfRepo.UpdateAsync(comment, token: default);

        // Assert
        var updatedComment = await BlogCommentEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(updatedComment);
        Assert.AreEqual("更新后的评论内容", updatedComment.Content);
        Assert.AreEqual(originalCreatedAt, updatedComment.CreatedAt);
        Assert.AreEqual(originalCreatedBy, updatedComment.CreatedBy);
        Assert.IsTrue(updatedComment.UpdatedAt.HasValue);
        Assert.AreEqual(1, updatedComment.UpdatedBy);
    }

    /// <summary>
    /// 测试软删除评论
    /// </summary>
    [TestMethod]
    public async Task SoftDeleteComment_ShouldSetDeleteFields()
    {
        // Arrange
        var comment = await BlogCommentEfRepo.GetAsync(1, noTracking: false, token: default);
        Assert.IsNotNull(comment);

        // Act
        comment.IsDeleted = true;
        await BlogCommentEfRepo.UpdateAsync(comment, token: default);

        // Assert
        var deletedComment = await BlogCommentEfRepo.GetAsync(1, token: default);
        Assert.IsNull(deletedComment); // 由于软删除过滤器，应该查不到
    }
}