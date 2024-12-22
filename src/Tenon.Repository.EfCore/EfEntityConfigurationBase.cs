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
    #region 常量定义

    /// <summary>
    /// 行版本属性名
    /// </summary>
    private const string RowVersionPropertyName = "RowVersion";

    /// <summary>
    /// 审计相关属性名
    /// </summary>
    private const string CreatedAtPropertyName = "CreatedAt";
    private const string CreatedByPropertyName = "CreatedBy";
    private const string UpdatedAtPropertyName = "UpdatedAt";
    private const string UpdatedByPropertyName = "UpdatedBy";
    private const string DeletedAtPropertyName = "DeletedAt";
    private const string DeletedByPropertyName = "DeletedBy";
    private const string IsDeletedPropertyName = "IsDeleted";

    #endregion

    #region 私有字段

    private readonly bool _isSoftDeleteEntity;
    private readonly bool _isConcurrencyEntity;
    private readonly bool _isAuditableEntity;
    private readonly bool _isFullAuditableEntity;

    #endregion

    #region 构造函数

    protected EfEntityConfigurationBase()
    {
        var entityType = typeof(TEntity);
        _isSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(entityType);
        _isConcurrencyEntity = typeof(IConcurrency).IsAssignableFrom(entityType);
        _isAuditableEntity = typeof(IAuditable<long>).IsAssignableFrom(entityType);
        _isFullAuditableEntity = typeof(IFullAuditable<long>).IsAssignableFrom(entityType);
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 配置实体类型
    /// </summary>
    /// <param name="builder">Entity Framework Core 实体类型构建器</param>
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // 配置基础属性
        ConfigureKey(builder);
        ConfigureBaseProperties(builder);

        // 配置审计属性
        if (_isAuditableEntity)
        {
            ConfigureAuditableProperties(builder);
        }

        if (_isFullAuditableEntity)
        {
            ConfigureFullAuditableProperties(builder);
        }

        // 配置软删除
        if (_isSoftDeleteEntity)
        {
            ConfigureSoftDelete(builder);
        }

        // 配置并发控制
        if (_isConcurrencyEntity)
        {
            ConfigureConcurrency(builder);
        }

        // 配置索引
        ConfigureIndexes(builder);

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

    #endregion

    #region 受保护的配置方法

    /// <summary>
    /// 配置基础属性
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureBaseProperties(EntityTypeBuilder<TEntity> builder)
    {
        // 配置实体的基础属性
        var entityType = typeof(TEntity);
        builder.ToTable(t => 
        {
            t.HasComment($"{entityType.Name}表");
        });
    }

    /// <summary>
    /// 配置审计属性
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureAuditableProperties(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(CreatedAtPropertyName)
            .IsRequired()
            .HasColumnOrder(90)
            .HasComment("创建时间");

        builder.Property(CreatedByPropertyName)
            .IsRequired()
            .HasColumnOrder(91)
            .HasComment("创建人ID");

        builder.Property(UpdatedAtPropertyName)
            .HasColumnOrder(92)
            .HasComment("更新时间");

        builder.Property(UpdatedByPropertyName)
            .HasColumnOrder(93)
            .HasComment("更新人ID");
    }

    /// <summary>
    /// 配置完整审计属性
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureFullAuditableProperties(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(DeletedAtPropertyName)
            .HasColumnOrder(94)
            .HasComment("删除时间");

        builder.Property(DeletedByPropertyName)
            .HasColumnOrder(95)
            .HasComment("删除人ID");
    }

    /// <summary>
    /// 配置软删除
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureSoftDelete(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(IsDeletedPropertyName)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnOrder(2)
            .HasComment("是否已删除");

        builder.HasQueryFilter(d => !EF.Property<bool>(d, IsDeletedPropertyName));
    }

    /// <summary>
    /// 配置主键
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureKey(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired()
            .HasColumnOrder(1)
            .ValueGeneratedOnAdd()
            .HasComment("主键ID");
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
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate()
            .HasComment("行版本号，用于并发控制");
    }

    /// <summary>
    /// 配置索引
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureIndexes(EntityTypeBuilder<TEntity> builder)
    {
        // 配置常用字段的索引
        if (_isSoftDeleteEntity)
        {
            builder.HasIndex(IsDeletedPropertyName)
                .HasFilter($"[{IsDeletedPropertyName}] = 0")
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_{IsDeletedPropertyName}");
        }

        if (_isAuditableEntity)
        {
            builder.HasIndex(CreatedAtPropertyName)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_{CreatedAtPropertyName}");
            
            builder.HasIndex(CreatedByPropertyName)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_{CreatedByPropertyName}");
        }
    }

    /// <summary>
    /// 提供额外的配置点，允许子类进行自定义配置
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    protected virtual void ConfigureExtra(EntityTypeBuilder<TEntity> builder)
    {
        // 子类可以通过重写此方法添加额外的配置
    }

    #endregion
}