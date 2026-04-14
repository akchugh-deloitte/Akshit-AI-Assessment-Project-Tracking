using System;
using System.Collections.Generic;
using System.Linq;
using ServiceApi.API.DTOs;

namespace ServiceApi.API.Utilities;

public static class EnumerableExtensions
{
    // Sorting for ProjectResponse collections
    public static IEnumerable<ProjectResponse> ApplyProjectSorting(
        this IEnumerable<ProjectResponse> items,
        string? sortBy,
        string? sortDir)
    {
        var sort = (sortBy ?? string.Empty).ToLowerInvariant();
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sort switch
        {
            "createdon" => desc ? items.OrderByDescending(p => p.CreatedOn) : items.OrderBy(p => p.CreatedOn),
            "name" => desc ? items.OrderByDescending(p => p.Name) : items.OrderBy(p => p.Name),
            "status" => desc ? items.OrderByDescending(p => p.Status) : items.OrderBy(p => p.Status),
            "issuecount" => desc ? items.OrderByDescending(p => p.IssueCount) : items.OrderBy(p => p.IssueCount),
            _ => items.OrderByDescending(p => p.CreatedOn)
        };
    }

    // Generic paging for IEnumerable
    public static IEnumerable<T> ApplyPaging<T>(
        this IEnumerable<T> items,
        int? pageNumber,
        int? pageSize)
    {
        var page = pageNumber.GetValueOrDefault(1);
        if (page < 1) page = 1;

        var size = pageSize.GetValueOrDefault(20);
        if (size < 1) size = 1;
        if (size > 100) size = 100;

        var skip = (page - 1) * size;
        return items.Skip(skip).Take(size);
    }
}
