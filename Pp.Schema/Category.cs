namespace Pp.Schema
{
    public class CategoryRequest
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Tags { get; set; }
    }
    public class CategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Tags { get; set; }
        public string InsertUser { get; set; }

    }
}
