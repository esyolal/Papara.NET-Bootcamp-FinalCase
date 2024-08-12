namespace Pp.Schema
{
    public class UserRequest
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public decimal Points { get; set; }
    }
    public class UserResponse
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public decimal Points { get; set; }
    }
}
