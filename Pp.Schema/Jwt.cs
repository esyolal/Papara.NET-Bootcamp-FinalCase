namespace Pp.Schema
{
    public class JwtResponse
    {
        public bool IsValid { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
