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
            Content = "这是一条回复评论",
            Commenter = "回复者",
            CommentTime = DateTime.Now,
            ParentId = parentComment.Id
        };

        // Act
        await BlogCommentEfRepo.InsertAsync(childComment, token: default);
        
        // 使用 Include 加载父评论和子评论
        var savedComment = await DbContext.BlogComments
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == childComment.Id);

        // Assert
        Assert.IsNotNull(savedComment);
        Assert.IsNotNull(savedComment.Parent);
        Assert.AreEqual(parentComment.Id, savedComment.Parent.Id);
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
    /// 测试更新评论
    /// </summary>
    [TestMethod]
    public async Task UpdateComment_ShouldUpdateSuccessfully()
    {
        // Arrange
        var comment = await BlogCommentEfRepo.GetAsync(1, token: default);
        Assert.IsNotNull(comment);

        // Act
        comment.Content = "更新后的评论内容";
        comment.LikeCount += 1;
        await BlogCommentEfRepo.UpdateAsync(comment, token: default);

        var updatedComment = await BlogCommentEfRepo.GetAsync(comment.Id, token: default);

        // Assert
        Assert.IsNotNull(updatedComment);
        Assert.AreEqual("更新后的评论内容", updatedComment.Content);
        Assert.AreEqual(comment.LikeCount, updatedComment.LikeCount);
    }

    /// <summary>
    /// 测试删除评论（包括子评论）
    /// </summary>
    [TestMethod]
    public async Task DeleteComment_ShouldDeleteWithChildren()
    {
        // Arrange
        var parentComment = await DbContext.BlogComments
            .Include(c => c.Children)
            .FirstAsync(c => c.Children.Any());

        var childrenIds = parentComment.Children.Select(c => c.Id).ToList();
        Assert.IsTrue(childrenIds.Count > 0, "测试数据中应该包含子评论");

        // Act
        DbContext.BlogComments.Remove(parentComment);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedParent = await BlogCommentEfRepo.GetAsync(parentComment.Id, token: default);
        Assert.IsNull(deletedParent, "父评论应该被删除");

        // 验证子评论也被删除
        foreach (var childId in childrenIds)
        {
            var deletedChild = await BlogCommentEfRepo.GetAsync(childId, token: default);
            Assert.IsNull(deletedChild, $"子评论 {childId} 应该被删除");
        }
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