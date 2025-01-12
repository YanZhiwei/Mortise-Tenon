using Microsoft.EntityFrameworkCore;
using Tenon.EntityFrameworkCore.Extensions.Models;

namespace Tenon.EntityFrameworkCore.Extensions.Tests;

[TestClass]
public class QueryableExtensionTests
{
    private TestDbContext _dbContext = null!;
    private List<TestEntity> _testData = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _dbContext = new TestDbContext(options);
        
        // 准备测试数据
        _testData = Enumerable.Range(1, 20)
            .Select(i => new TestEntity { Id = i, Name = $"Test{i}" })
            .ToList();
            
        _dbContext.TestEntities.AddRange(_testData);
        _dbContext.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task ToPagedListAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var query = _dbContext.TestEntities.OrderBy(x => x.Id);
        var pageIndex = 2;
        var pageSize = 5;

        // Act
        var result = await query.ToPagedListAsync(pageIndex, pageSize);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(pageSize, result.CurrentCount);
        Assert.AreEqual(20, result.TotalCount);
        Assert.AreEqual(pageIndex, result.CurrentPage);
        Assert.AreEqual(pageSize, result.PageSize);
        Assert.AreEqual(4, result.TotalPages);
        
        // 验证分页数据正确性
        var expectedIds = Enumerable.Range(6, 5);
        CollectionAssert.AreEqual(expectedIds.ToList(), result.Data.Select(x => x.Id).ToList());
    }

    [TestMethod]
    public async Task ToPagedListAsync_WithEmptySource_ShouldReturnEmptyList()
    {
        // Arrange
        await _dbContext.Database.EnsureDeletedAsync();
        var query = _dbContext.TestEntities;

        // Act
        var result = await query.ToPagedListAsync(1, 10);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.CurrentCount);
        Assert.AreEqual(0, result.TotalCount);
        Assert.AreEqual(0, result.CurrentPage);
        Assert.AreEqual(0, result.PageSize);
        Assert.AreEqual(0, result.TotalPages);
    }

    [TestMethod]
    public void WhereIf_WhenConditionTrue_ShouldApplyFilter()
    {
        // Arrange
        var condition = true;
        var query = _dbContext.TestEntities.AsQueryable();

        // Act
        var result = query.WhereIf(condition, x => x.Id > 10).ToList();

        // Assert
        Assert.AreEqual(10, result.Count);
        Assert.IsTrue(result.All(x => x.Id > 10));
    }

    [TestMethod]
    public void WhereIf_WhenConditionFalse_ShouldNotApplyFilter()
    {
        // Arrange
        var condition = false;
        var query = _dbContext.TestEntities.AsQueryable();

        // Act
        var result = query.WhereIf(condition, x => x.Id > 10).ToList();

        // Assert
        Assert.AreEqual(20, result.Count);
    }
}

/// <summary>
/// 测试用实体类
/// </summary>
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

/// <summary>
/// 测试用 DbContext
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
} 