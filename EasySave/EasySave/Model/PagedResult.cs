using System;
using System.Collections.Generic;

namespace EasySave.Model
{
    /// <summary>
    /// Represents a paginated result set.
    ///
    /// Contains:
    /// - The items for the current page
    /// - The total number of items
    /// - Pagination metadata (page index, page size, total pages)
    ///
    /// PageIndex is 1-based (1 = first page).
    /// </summary>
    public sealed class PagedResult<T>
    {
        /// <summary>
        /// Items belonging to the current page.
        /// </summary>
        public required IReadOnlyList<T> Items { get; init; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public required int TotalCount { get; init; }

        /// <summary>
        /// Current page index (1-based).
        /// </summary>
        public required int PageIndex { get; init; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public required int PageSize { get; init; }

        /// <summary>
        /// Total number of available pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Indicates whether a previous page exists.
        /// </summary>
        public bool HasPrevious => PageIndex > 1;

        /// <summary>
        /// Indicates whether a next page exists.
        /// </summary>
        public bool HasNext => PageIndex < TotalPages;
    }
}
