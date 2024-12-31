using System;
using System.Collections.Generic;
using Tenon.Abstractions;
using Tenon.AspNetCore.DTOs.Request;

namespace Tenon.Models.Dtos;

[Serializable]
public class PagedResultDto<T> : IDto
    where T : class, new()
{
    private IReadOnlyList<T> _data = Array.Empty<T>();

    public PagedResultDto()
    {
    }

    public PagedResultDto(SearchPagedDto search)
        : this(search, Array.Empty<T>(), default)
    {
    }

    public PagedResultDto(SearchPagedDto search, IReadOnlyList<T> data, int count)
        : this(search.PageIndex, search.PageSize, data, count)
    {
    }

    public PagedResultDto(int pageIndex, int pageSize, IReadOnlyList<T> data, int count)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = count;
        Data = data;
    }

    public IReadOnlyList<T> Data
    {
        get => _data;
        set => _data = value ?? Array.Empty<T>();
    }

    public int RowsCount => _data.Count;

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int PageCount => (RowsCount + PageSize - 1) / PageSize;
}