﻿using System;
using System.Collections.Generic;
using Tenon.Abstractions;

namespace Tenon.Models.Dtos;

[Serializable]
public class PagedListDto<T> : IDto
    where T : class, new()
{
    private IReadOnlyList<T> _data = Array.Empty<T>();

    public PagedListDto()
    {
    }

    public PagedListDto(SearchPagedDto search)
        : this(search, Array.Empty<T>(), default)
    {
    }

    public PagedListDto(SearchPagedDto search, IReadOnlyList<T> data, int count)
        : this(search.PageIndex, search.PageSize, data, count)
    {
    }

    public PagedListDto(int pageIndex, int pageSize, IReadOnlyList<T> data, int count)
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