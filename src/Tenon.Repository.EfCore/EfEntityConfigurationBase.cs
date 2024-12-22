using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Tenon.Repository.EfCore;

/// <summary>
/// Entity Framework Core 实体配置的抽象基类
/// </summary>
public abstract class EfEntityConfigurationBase
{
    /// <summary>
    /// 配置实体类型
    /// </summary>
    /// <param name="modelBuilder">Entity Framework Core 模型构建器</param>
    public abstract void Configure(ModelBuilder modelBuilder);
}

/// <summary>
/// Entity Framework Core 泛型实体配置的抽象基类
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public abstract class EfEntityConfigurationBase<TEntity> : EfEntityConfigurationBase,
    IEntityTypeConfiguration<TEntity>
    where TEntity : EfEntity
{
    private const string RowVersionPropertyName = "RowVersion";
    private readonly bool _isSoftDeleteEntity;
    private readonly bool _isConcurrencyEntity;

    protected EfEntityConfigurationBase()
    {
        var entityType = typeof(TEntity);
        _isSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(entityType);
        _isConcurrencyEntity = typeof(IConcurrency).IsAssignableFrom(entityType);
    }

    /// <summary>
    /// 配置实体类型
    /// </summary>
    /// <param name="builder">Entity Framework Core 实体类型构建器</param>
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        // 配置基础属性
        ConfigureKey(builder);
        
        // 配置软删除
        if (_isSoftDeleteEntity)
        {
            ConfigureQueryFilter(builder);
        }
        
        // 配置并发控制
        if (_isConcurrencyEntity)
        {
            ConfigureConcurrency(builder);
        }

        // 允许子类进行额外配置
        ConfigureExtra(builder);
    }

    /// <summary>
    /// 配置模型构建器
    /// </summary>
    /// <param name="modelBuilder">Entity Framework Core 模型构建器</param>
    public override void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        Configure(modelBuilder.Entity<TEntity>());
    }

    /// <summary>
    /// 配置软删除查询过滤器
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureQueryFilter(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(nameof(ISoftDelete.IsDeleted))
            .HasDefaultValue(false)
            .HasColumnOrder(2);
        builder.HasQueryFilter(d => !EF.Property<bool>(d, nameof(ISoftDelete.IsDeleted)));
    }

    /// <summary>
    /// 配置主键
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureKey(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnOrder(1)
            .ValueGeneratedOnAdd();
    }

    /// <summary>
    /// 配置并发控制
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureConcurrency(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(RowVersionPropertyName)
            .IsRequired()
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate();
    }

    /// <summary>
    /// 提供额外的配置点，允许子类进行自定义配置
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureExtra(EntityTypeBuilder<TEntity> builder)
    {
        // 子类可以通过重写此方法添加额外的配置
    }
}