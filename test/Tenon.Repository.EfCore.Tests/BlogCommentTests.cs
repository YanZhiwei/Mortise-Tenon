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
        var blog = await BlogEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(blog);

        var comment = new BlogComment
        {
            BlogId = blog.Id,
            Content = "这是一条新评论",
            Commenter = "新评论者",
            CommentTime = DateTime.Now
        };

        // Act
        await BlogCommentEfRepo.InsertAsync(comment, token: default);
        var savedComment = await BlogCommentEfRepo.GetAsync(comment.Id, token: default);

        // Assert
        Assert.IsNotNull(savedComment);
        Assert.AreEqual(comment.Content, savedComment.Content);
        Assert.AreEqual(comment.Commenter, savedComment.Commenter);
        Assert.AreEqual(1, savedComment.CreatedBy);
        Assert.IsTrue(savedComment.CreatedAt > DateTimeOffset.MinValue);
        Assert.IsFalse(savedComment.IsDeleted);
        Assert.IsNull(savedComment.UpdatedAt);
        Assert.IsNull(savedComment.UpdatedBy);
        Assert.IsNull(savedComment.DeletedAt);
        Assert.IsNull(savedComment.DeletedBy);
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
        Assert.IsFalse(updatedComment.IsDeleted);
        Assert.IsNull(updatedComment.DeletedAt);
        Assert.IsNull(updatedComment.DeletedBy);
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

        var originalCreatedAt = comment.CreatedAt;
        var originalCreatedBy = comment.CreatedBy;

        // Act
        comment.IsDeleted = true;
        await BlogCommentEfRepo.UpdateAsync(comment, token: default);

        // Assert
        var deletedComment = await DbContext.BlogComments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
        Assert.IsNotNull(deletedComment);
        Assert.IsTrue(deletedComment.IsDeleted);
        Assert.AreEqual(originalCreatedAt, deletedComment.CreatedAt);
        Assert.AreEqual(originalCreatedBy, deletedComment.CreatedBy);
        Assert.IsTrue(deletedComment.DeletedAt.HasValue);
        Assert.AreEqual(1, deletedComment.DeletedBy);
    }

    /// <summary>
    /// 测试创建子评论
    /// </summary>
    [TestMethod]
    public async Task CreateChildComment_ShouldCreateSuccessfully()
    {
        // Arrange
        var parentComment = await BlogCommentEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(parentComment);

        var childComment = new BlogComment
        {
            BlogId = parentComment.BlogId,
            Content = "这是一条子评论",
            Commenter = "子评论者",
            CommentTime = DateTime.Now,
            ParentId = parentComment.Id
        };

        // Act
        await BlogCommentEfRepo.InsertAsync(childComment, token: default);

        // Assert
        var savedComment = await BlogCommentEfRepo.GetAsync(childComment.Id, token: default);
        Assert.IsNotNull(savedComment);
        Assert.AreEqual(childComment.Content, savedComment.Content);
        Assert.AreEqual(childComment.ParentId, savedComment.ParentId);
        Assert.AreEqual(1, savedComment.CreatedBy);
        Assert.IsTrue(savedComment.CreatedAt > DateTimeOffset.MinValue);
        Assert.IsFalse(savedComment.IsDeleted);
        Assert.IsNull(savedComment.UpdatedAt);
        Assert.IsNull(savedComment.UpdatedBy);
        Assert.IsNull(savedComment.DeletedAt);
        Assert.IsNull(savedComment.DeletedBy);
    }

    /// <summary>
    /// 测试获取博客的所有评论（包括子评论）
    /// </summary>
    [TestMethod]
    public async Task GetBlogComments_ShouldReturnAllComments()
    {
        // Arrange
        var blog = await BlogEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(blog);

        // Act
        var comments = await DbContext.BlogComments
            .Include(c => c.Children)
            .Where(c => c.BlogId == blog.Id && c.ParentId == null)
            .ToListAsync();

        // Assert
        Assert.IsTrue(comments.Count > 0);
        Assert.IsTrue(comments.Any(c => c.Children.Any())); // 确保有子评论
    }

    /// <summary>
    /// 测试删除评论（包括子评论）
    /// </summary>
    [TestMethod]
    public async Task DeleteComment_ShouldDeleteWithChildren()
    {
        // Arrange
        var parentComment = await BlogCommentEfRepo.GetAsync(1, noTracking: false, token: default);
        Assert.IsNotNull(parentComment);

        // 确保父评论有子评论
        var childComments = await DbContext.BlogComments
            .Where(c => c.ParentId == parentComment.Id)
            .ToListAsync();
        Assert.IsTrue(childComments.Count > 0, "父评论应该有子评论");

        // Act
        parentComment.IsDeleted = true;
        await BlogCommentEfRepo.UpdateAsync(parentComment, token: default);

        // Assert
        // 使用 GetAsync 方法验证评论是否被软删除（应该查不到）
        var deletedParentComment = await BlogCommentEfRepo.GetAsync(parentComment.Id, token: default);
        Assert.IsNull(deletedParentComment, "父评论应该被删除");

        // 使用 IgnoreQueryFilters 验证评论是否真实存在但被标记为删除
        var softDeletedParentComment = await DbContext.BlogComments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == parentComment.Id);
        Assert.IsNotNull(softDeletedParentComment, "父评论应该存在但被标记为删除");
        Assert.IsTrue(softDeletedParentComment.IsDeleted, "父评论应该被标记为删除");
        Assert.IsTrue(softDeletedParentComment.DeletedAt.HasValue, "父评论的删除时间应该被设置");
        Assert.AreEqual(1, softDeletedParentComment.DeletedBy, "父评论的删除者应该被设置");

        // 验证子评论是否也被软删除
        var remainingChildComments = await BlogCommentEfRepo.GetListAsync(
            c => c.ParentId == parentComment.Id,
            token: default);
        Assert.IsFalse(remainingChildComments.Any(), "子评论应该被删除");

        // 验证子评论是否真实存在但被标记为删除
        var softDeletedChildComments = await DbContext.BlogComments
            .IgnoreQueryFilters()
            .Where(c => c.ParentId == parentComment.Id)
            .ToListAsync();
        Assert.IsTrue(softDeletedChildComments.All(c => c.IsDeleted), "所有子评论都应该被标记为删除");
        Assert.IsTrue(softDeletedChildComments.All(c => c.DeletedAt.HasValue), "所有子评论的删除时间都应该被设置");
        Assert.IsTrue(softDeletedChildComments.All(c => c.DeletedBy == 1), "所有子评论的删除者都应该被设置");
    }

    /// <summary>
    /// 测试获取用户的所有评论
    /// </summary>
    [TestMethod]
    public async Task GetUserComments_ShouldReturnCorrectComments()
    {
        // Arrange
        var commenter = "王五";

        // Act
        var comments = await BlogCommentEfRepo.GetListAsync(
            c => c.Commenter == commenter,
            token: default);
        var commentList = comments.ToList();

        // Assert
        Assert.IsTrue(commentList.Count > 0);
        Assert.IsTrue(commentList.All(c => c.Commenter == commenter));
    }

    /// <summary>
    /// 测试按时间排序获取评论
    /// </summary>
    [TestMethod]
    public async Task GetCommentsByTime_ShouldReturnOrderedComments()
    {
        // Act
        var comments = await DbContext.BlogComments
            .OrderByDescending(c => c.CommentTime)
            .ToListAsync();

        // Assert
        Assert.IsTrue(comments.Count > 0);
        for (int i = 1; i < comments.Count; i++)
        {
            Assert.IsTrue(comments[i - 1].CommentTime >= comments[i].CommentTime);
        }
    }
}