using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using System;

// Aliases to avoid ambiguity
using MyRegisterRequest = UnitySimBackend.Models.DTOs.RegisterRequest;
using MyLoginRequest = UnitySimBackend.Models.DTOs.LoginRequest;
using MyUserResponse = UnitySimBackend.Models.DTOs.UserResponse;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace UnitySimBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _jwtKey;

        public AuthController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SimulationDB");
            _jwtKey = config["Jwt:Key"] ?? throw new Exception("JWT key not configured");
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Register([FromBody] MyRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password required." });

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username=@u", conn);
            checkCmd.Parameters.AddWithValue("@u", request.Username);
            var exists = (int)checkCmd.ExecuteScalar() > 0;
            if (exists)
                return Conflict(new { message = "Username already exists." });

            using var cmd = new SqlCommand(@"
                INSERT INTO Users (Username, Password, FirstName, LastName, Permission)
                VALUES (@u, @p, @f, @l, @perm)", conn);

            cmd.Parameters.AddWithValue("@u", request.Username);
            cmd.Parameters.AddWithValue("@p", hashedPassword);
            cmd.Parameters.AddWithValue("@f", request.FirstName ?? "");
            cmd.Parameters.AddWithValue("@l", request.LastName ?? "");
            cmd.Parameters.AddWithValue("@perm", request.Permission ?? "User");
            cmd.ExecuteNonQuery();

            return Ok(new { message = "User registered successfully." });
        }

        /// <summary>
        /// Logs in a user and returns JWT token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(MyUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<MyUserResponse> Login([FromBody] MyLoginRequest request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("SELECT Id, Password, FirstName, LastName, Permission FROM Users WHERE Username=@u", conn);
            cmd.Parameters.AddWithValue("@u", request.Username);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return Unauthorized(new { message = "Invalid username or password" });

            var storedHash = reader.GetString(1);
            if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                return Unauthorized(new { message = "Invalid username or password" });

            var id = reader.GetInt32(0);
            var firstName = reader.GetString(2);
            var lastName = reader.GetString(3);
            var permission = reader.GetString(4);

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", id.ToString()),
                    new Claim("username", request.Username),
                    new Claim("permission", permission)
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            var response = new MyUserResponse
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Permission = permission,
                Token = jwt
            };

            return Ok(response);
        }
    }
}
