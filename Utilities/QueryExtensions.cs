using System;
using System.Linq;
using ServiceApi.API.DTOs;
using ServiceApi.API.Models;

namespace ServiceApi.API.Utilities;

public static class QueryExtensions
{
    // Issue-specific filters
    public static IQueryable<Issue> ApplyIssueFilters(this IQueryable<Issue> query, IssueFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<IssueStatus>(filter.Status, true, out var status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.Priority) &&
            Enum.TryParse<IssuePriority>(filter.Priority, true, out var priority))
        {
            query = query.Where(i => i.Priority == priority);
        }

        if (filter.AssigneeId.HasValue)
            query = query.Where(i => i.AssigneeId == filter.AssigneeId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(i => i.Title.Contains(filter.Search));

        if (filter.DueBefore.HasValue)
            query = query.Where(i => i.DueDate <= filter.DueBefore);

        return query;
    }

    // Issue-specific sorting
    public static IQueryable<Issue> ApplyIssueSorting(this IQueryable<Issue> query, IssueFilterRequest filter)
    {
        var sortBy = (filter.SortBy ?? string.Empty).ToLowerInvariant();
        var desc = string.Equals(filter.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        // Default sorting when none specified
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(i => i.CreatedOn);

        return sortBy switch
        {
            "createdon" => desc ? query.OrderByDescending(i => i.CreatedOn) : query.OrderBy(i => i.CreatedOn),
            "updatedon" => desc ? query.OrderByDescending(i => i.UpdatedOn) : query.OrderBy(i => i.UpdatedOn),
            "duedate" => desc ? query.OrderByDescending(i => i.DueDate) : query.OrderBy(i => i.DueDate),
            "priority" => desc ? query.OrderByDescending(i => i.Priority) : query.OrderBy(i => i.Priority),
            "status" => desc ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            "title" => desc ? query.OrderByDescending(i => i.Title) : query.OrderBy(i => i.Title),
            "assigneename" => desc
                ? query.OrderByDescending(i => i.Assignee != null ? i.Assignee.Username : null)
                : query.OrderBy(i => i.Assignee != null ? i.Assignee.Username : null),
            _ => desc ? query.OrderByDescending(i => i.CreatedOn) : query.OrderBy(i => i.CreatedOn)
        };
    }

    // Generic paging for IQueryable
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int? pageNumber, int? pageSize)
    {
        var page = pageNumber.GetValueOrDefault(1);
        if (page < 1) page = 1;

        var size = pageSize.GetValueOrDefault(20);
        if (size < 1) size = 1;
        if (size > 100) size = 100;

        var skip = (page - 1) * size;
        return query.Skip(skip).Take(size);
    }
}
