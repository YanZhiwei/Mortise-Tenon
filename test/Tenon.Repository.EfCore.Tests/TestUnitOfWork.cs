using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tenon.Repository.EfCore.Transaction;

namespace Tenon.Repository.EfCore.Tests;

/// <summary>
/// 测试用的工作单元实现
/// </summary>
public class TestUnitOfWork : UnitOfWork
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public TestUnitOfWork(DbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// 获取数据库事务
    /// </summary>
    /// <param name="isolationLevel">事务隔离级别</param>
    protected override IDbContextTransaction GetDbTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return DbContext.Database.BeginTransaction(isolationLevel);
    }
}