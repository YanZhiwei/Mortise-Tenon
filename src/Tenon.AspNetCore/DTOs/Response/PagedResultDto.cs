using Tenon.Abstractions;

namespace Tenon.AspNetCore.DTOs.Response;

/// <summary>
/// 分页查询结果数据传输对象
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PagedResultDto<T> : IDto
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页数据
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => Total > (PageIndex * PageSize);

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;
} 