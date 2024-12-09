using System;
using System.Collections.Generic;

namespace Tenon.Repository.EfCore;

/// <summary>
/// 分页结果
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public class PagedResult<TEntity>
{
    /// <summary>
    /// 当前页的项目集合
    /// </summary>
    public IEnumerable<TEntity>? Items { get; set; }

    /// <summary>
    /// 总项目数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页索引
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页的项目数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}