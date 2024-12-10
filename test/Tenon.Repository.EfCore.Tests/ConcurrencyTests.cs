using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

[TestClass]
public class ConcurrencyTests : TestBase
{
    /// <summary>
    ///     测试创建并发实体
    /// </summary>
    [TestMethod]
    public async Task CreateConcurrentEntity_ShouldGenerateRowVersion()
    {
        // Arrange
        var entity = new ConcurrentEntity { Name = "测试实体" };

        // Act
        await ConcurrentEfRepo.InsertAsync(entity, default);

        // Assert
        var savedEntity = await ConcurrentEfRepo.GetAsync(entity.Id, default);
        Assert.IsNotNull(savedEntity);
        Assert.AreEqual(entity.Name, savedEntity.Name);
        Assert.IsNotNull(savedEntity.RowVersion);
        Assert.IsTrue(savedEntity.RowVersion.Length > 0);
    }

    /// <summary>
    ///     测试并发更新冲突
    /// </summary>
    [TestMethod]
    public async Task UpdateConcurrentEntity_ShouldThrowException_WhenConcurrencyConflict()
    {
        // Arrange
        var entity = new ConcurrentEntity { Name = "测试实体" };
        await ConcurrentEfRepo.InsertAsync(entity, default);

        // 模拟两个用户同时获取同一个实体
        var user1Entity = await ConcurrentEfRepo.GetAsync(entity.Id, false, default);
        var user2Entity = await ConcurrentEfRepo.GetAsync(entity.Id, false, default);

        Assert.IsNotNull(user1Entity);
        Assert.IsNotNull(user2Entity);

        // Act & Assert
        // 用户1更新实体
        user1Entity.Name = "用户1更新";
        await ConcurrentEfRepo.UpdateAsync(user1Entity, default);

        user2Entity.Name = "用户2更新";
        await ConcurrentEfRepo.UpdateAsync(user2Entity, default);
    }

    /// <summary>
    ///     测试正常更新（无并发冲突）
    /// </summary>
    [TestMethod]
    public async Task UpdateConcurrentEntity_ShouldSucceed_WhenNoConflict()
    {
        // Arrange
        var entity = new ConcurrentEntity { Name = "测试实体" };
        await ConcurrentEfRepo.InsertAsync(entity, default);

        // Act
        var entityToUpdate = await ConcurrentEfRepo.GetAsync(entity.Id, false, default);
        Assert.IsNotNull(entityToUpdate);

        var originalRowVersion = entityToUpdate.RowVersion;
        entityToUpdate.Name = "更新后的名称";
        await ConcurrentEfRepo.UpdateAsync(entityToUpdate, default);

        // Assert
        var updatedEntity = await ConcurrentEfRepo.GetAsync(entity.Id, default);
        Assert.IsNotNull(updatedEntity);
        Assert.AreEqual("更新后的名称", updatedEntity.Name);
        CollectionAssert.AreNotEqual(originalRowVersion, updatedEntity.RowVersion);
    }

    /// <summary>
    ///     测试删除并发实体
    /// </summary>
    [TestMethod]
    public async Task DeleteConcurrentEntity_ShouldSucceed()
    {
        // Arrange
        var entity = new ConcurrentEntity { Name = "测试实体" };
        await ConcurrentEfRepo.InsertAsync(entity, default);

        // Act
        var entityToDelete = await ConcurrentEfRepo.GetAsync(entity.Id, false, default);
        Assert.IsNotNull(entityToDelete);

        await ConcurrentEfRepo.RemoveAsync(entityToDelete, default);

        // Assert
        var deletedEntity = await ConcurrentEfRepo.GetAsync(entity.Id, default);
        Assert.IsNull(deletedEntity);
    }
}