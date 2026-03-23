using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Repositories
{
    /// <summary>
    /// Extension method for pagination support on IQueryable
    /// </summary>
    public static class PaginationExtensions
    {
        public static async Task<PaginatedResult<T>> PaginateAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize) where T : class
        {
            int totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>(items, pageNumber, pageSize, totalItems);
        }
    }
}
