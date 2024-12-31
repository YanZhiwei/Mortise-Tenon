using Tenon.Abstractions;
using System.ComponentModel;

namespace Tenon.AspNetCore.DTOs.Request;

/// <summary>
/// 分页查询基础数据传输对象
/// </summary>
/// <remarks>
/// 提供基础的分页查询参数，包括页码和每页大小。
/// 默认每页最小显示 5 条，最大显示 100 条数据。
/// </remarks>
public abstract class SearchPagedDto : IDto
{
    /// <summary>
    /// 默认页码
    /// </summary>
    protected const int DefaultPageIndex = 1;

    /// <summary>
    /// 默认每页大小
    /// </summary>
    protected const int DefaultPageSize = 20;

    /// <summary>
    /// 每页最小记录数
    /// </summary>
    protected const int MinPageSize = 5;

    /// <summary>
    /// 每页最大记录数
    /// </summary>
    protected const int MaxPageSize = 100;

    private int _pageIndex = DefaultPageIndex;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// 获取或设置页码
    /// </summary>
    /// <value>页码，从1开始</value>
    /// <remarks>
    /// 如果设置的值小于1，将返回默认值1
    /// </remarks>
    [Description("页码，从1开始")]
    public int PageIndex
    {
        get => _pageIndex < DefaultPageIndex ? DefaultPageIndex : _pageIndex;
        set => _pageIndex = value;
    }

    /// <summary>
    /// 获取或设置每页显示的记录数
    /// </summary>
    /// <value>每页记录数，范围：5-100</value>
    /// <remarks>
    /// 如果设置的值小于5，将返回最小值5
    /// 如果设置的值大于100，将返回最大值100
    /// </remarks>
    [Description("每页记录数，范围：5-100")]
    public int PageSize
    {
        get => _pageSize switch
        {
            < MinPageSize => MinPageSize,
            > MaxPageSize => MaxPageSize,
            _ => _pageSize
        };
        set => _pageSize = value;
    }
} 