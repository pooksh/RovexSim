namespace UnitySimBackend.Models.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Permission { get; set; } = "User";
    }
}
