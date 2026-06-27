namespace WebAPIDevSecOps.Dto
{
    public class QueryParams
    {
        private int _pageNumber = 1;
        private int _pageSize = 20;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 1 : value > 100 ? 100 : value;
        }
    }

    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, QueryParams parameters)
        {
            return query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
                       .Take(parameters.PageSize);
        }
    }
}
