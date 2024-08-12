namespace Pp.Schema
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public string Features { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal PointsPercentage { get; set; }
        public decimal MaxPoints { get; set; }
        public List<long> CategoryIds { get; set; } = new List<long>();
    }

    public class ProductResponse
    {

        public string Name { get; set; }
        public string Features { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal PointsPercentage { get; set; }
        public decimal MaxPoints { get; set; }
        public List<string> CategoryNames { get; set; }
    }
}