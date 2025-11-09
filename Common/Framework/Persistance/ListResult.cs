namespace Framework.Persistance
{
    public class ListResult<T>
    {
        public IReadOnlyList<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}