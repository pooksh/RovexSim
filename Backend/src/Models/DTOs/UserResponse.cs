namespace UnitySimBackend.Models.DTOs
{
	public class UserResponse
	{
		public int Id { get; set; }
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string Permission { get; set; } = "";
		public string Token { get; set; } = ""; // JWT token for login
	}
}
