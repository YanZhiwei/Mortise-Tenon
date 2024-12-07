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
        
        // 清除跟踪器
        DbContext.ChangeTracker.Clear();
        
        // 获取标签及其关联的博客
        var tag = await DbContext.BlogTags
            .Include(t => t.Blogs)
            .FirstAsync(t => t.Name == tagName);
            
        Assert.IsNotNull(tag);
        
        // Act
        var blogs = await DbContext.Blogs
            .Include(b => b.Tags)
            .Where(b => b.Tags.Any(t => t.Id == tag.Id))
            .OrderBy(b => b.Id)
            .ToListAsync();

        // Assert
        Assert.AreEqual(2, blogs.Count);
        Assert.IsTrue(blogs.All(b => b.Tags.Any(t => t.Name == tagName)));
        
        // 验证具体的博客标题
        var blogTitles = blogs.Select(b => b.Title).ToList();
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

        // 先创建博客
        await BlogEfRepo.InsertAsync(blog, token: default);

        // 获取要关联的标签ID
        var tagIds = await DbContext.BlogTags
            .Where(t => t.Name == "技术" || t.Name == "生活")
            .Select(t => t.Id)
            .ToListAsync();

        // 清除 DbContext 跟踪的所有实体
        DbContext.ChangeTracker.Clear();

        // 重新加载博客
        var blogToUpdate = await DbContext.Blogs
            .Include(b => b.Tags)
            .FirstAsync(b => b.Id == blog.Id);

        // 加载要关联的标签
        var tagsToAdd = await DbContext.BlogTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync();

        // 建立关联关系
        foreach (var tag in tagsToAdd)
        {
            blogToUpdate.Tags.Add(tag);
        }

        // 保存更改
        await DbContext.SaveChangesAsync();

        // 重新加载博客及其标签进行验证
        var savedBlog = await DbContext.Blogs
            .Include(b => b.Tags)
            .FirstAsync(b => b.Id == blog.Id);

        // Assert
        Assert.IsNotNull(savedBlog);
        Assert.AreEqual(blog.Title, savedBlog.Title);
        Assert.AreEqual(2, savedBlog.Tags.Count);
        Assert.IsTrue(savedBlog.Tags.Any(t => t.Name == "技术"));
        Assert.IsTrue(savedBlog.Tags.Any(t => t.Name == "生活"));
    }

    /// <summary>
    /// 测试更新博客及其标签
    /// </summary>
    [TestMethod]
    public async Task UpdateBlogWithTags_ShouldUpdateSuccessfully()
    {
        // Arrange
        var blogId = 1L;

        // 清除跟踪器
        DbContext.ChangeTracker.Clear();

        // 加载博客及其标签
        var blog = await DbContext.Blogs
            .Include(b => b.Tags)
            .FirstAsync(b => b.Id == blogId);

        // 获取要更新的标签
        var newTagId = await DbContext.BlogTags
            .Where(t => t.Name == "生活")
            .Select(t => t.Id)
            .FirstAsync();

        // 更新博客基本信息
        blog.Title = "更新后的标题";

        // 清除现有标签
        blog.Tags.Clear();

        // 加载新标签
        var newTag = await DbContext.BlogTags.FindAsync(newTagId);
        Assert.IsNotNull(newTag);

        // 添加新标签
        blog.Tags.Add(newTag);

        // 保存更改
        await DbContext.SaveChangesAsync();

        // 重新加载博客进行验证
        DbContext.ChangeTracker.Clear();
        var updatedBlog = await DbContext.Blogs
            .Include(b => b.Tags)
            .FirstAsync(b => b.Id == blogId);

        // Assert
        Assert.IsNotNull(updatedBlog);
        Assert.AreEqual("更新后的标题", updatedBlog.Title);
        Assert.AreEqual(1, updatedBlog.Tags.Count);
        Assert.AreEqual("生活", updatedBlog.Tags.First().Name);
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
    /// 测试分页查询博客
    /// </summary>
    [TestMethod]
    public async Task GetBlogsByPage_ShouldReturnCorrectBlogs()
    {
        // Arrange
        var pageSize = 2;
        var skipCount = 1;

        // Act
        var blogs = BlogEfRepo.Where(b => true)
            .OrderByDescending(b => b.PublishTime)
            .Skip(skipCount)
            .Take(pageSize);
        var blogList = await blogs.ToListAsync();

        // Assert
        Assert.AreEqual(pageSize, blogList.Count);
        Assert.IsTrue(blogList[0].PublishTime > blogList[1].PublishTime);
    }

    /// <summary>
    /// 测试获取博客详情（包含标签和评论）
    /// </summary>
    [TestMethod]
    public async Task GetBlogDetail_ShouldIncludeTagsAndComments()
    {
        // Arrange
        var blogId = 1L; // 使用测试数据中的第一篇博客

        // Act
        var blog = await DbContext.Blogs
            .Include(b => b.Tags)
            .Include(b => b.Comments)
                .ThenInclude(c => c.Children)
            .FirstOrDefaultAsync(b => b.Id == blogId);

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

            // 如果是父评论，验证其子评论
            if (comment.Children.Any())
            {
                foreach (var childComment in comment.Children)
                {
                    Assert.AreEqual(comment.Id, childComment.ParentId, "子评论的 ParentId 应该匹配父评论的 Id");
                    Assert.IsFalse(string.IsNullOrEmpty(childComment.Content), "子评论内容不应为空");
                }
            }
        }

        // 验证评论数量
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
        var blogs = BlogEfRepo.Where(b => true)
            .OrderByDescending(b => b.ReadCount);
        var blogList = await blogs.ToListAsync();

        // Assert
        Assert.IsTrue(blogList.Count > 0);
        for (int i = 1; i < blogList.Count; i++)
        {
            Assert.IsTrue(blogList[i - 1].ReadCount >= blogList[i].ReadCount);
        }
    }
} 