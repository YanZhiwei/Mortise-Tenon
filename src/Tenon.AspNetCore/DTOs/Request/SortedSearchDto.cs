namespace Tenon.AspNetCore.DTOs.Request;

using System.ComponentModel;

/// <summary>
/// 排序查询基础数据传输对象
/// </summary>
/// <remarks>
/// 提供基础的排序查询参数，包括排序字段和排序方向。
/// </remarks>
public abstract class SortedSearchDto : SearchPagedDto
{
    /// <summary>
    /// 排序字段
    /// </summary>
    /// <remarks>
    /// 指定需要排序的字段名称
    /// </remarks>
    [Description("排序字段名称")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// 是否降序排序
    /// </summary>
    /// <remarks>
    /// true 表示降序，false 表示升序
    /// </remarks>
    [Description("是否降序排序")]
    public bool IsDescending { get; set; }

    /// <summary>
    /// 多字段排序
    /// </summary>
    /// <remarks>
    /// 格式：字段名:asc|desc,字段名:asc|desc
    /// 示例：name:asc,age:desc
    /// </remarks>
    [Description("多字段排序，格式：字段名:asc|desc,字段名:asc|desc")]
    public string? OrderByMultiple { get; set; }
} 