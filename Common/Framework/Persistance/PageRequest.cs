namespace Framework.Persistance
{
    public class PageRequest(int pageNumber = 1, int pageSize = 10)
    {
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
    }
}