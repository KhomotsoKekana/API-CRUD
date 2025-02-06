using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserApi.Models;
using UserApi.Models.Dto;
using UserApi.Models.Tokens;
using UserApi.Repositories;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUser _userInterface;
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<string, RefreshToken> _refreshTokens = new();

        public AuthController(IUser userRepo, IConfiguration configuration)
        {
             _userInterface = userRepo;
             _configuration = configuration;
        }
        [Authorize]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AddUser user)
        {
            if (user == null)
            {
                Log.Warning("User creation failed. User data is null.");
                return BadRequest(new { message = "User cannot be null." });
            }

            try
            {
                var exists = await _userInterface.GetUserByUserName(user.UserName);

                if (exists != null)
                {
                    Log.Warning("Username \"{Username}\" is already taken.", user.UserName);
                    return Conflict(new { message = $"Username \"{user.UserName}\" is already taken." });
                }
               

                if (await _userInterface.GetUserByEmail(user.Email) != null)
                {
                    Log.Warning("Email \"{Email}\" is already registered.", user.Email);
                    return Conflict(new { message = $"Email \"{user.Email}\" is already registered." });
                }

                var success = await _userInterface.InsertUser(user);
                if (success)
                {
                    return Ok(new { message = "User created successfully" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occured while creating the user" });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login loginDto)
        {
            Log.Information("Attempting login for email: {Email}", loginDto.Email);
            var user = await _userInterface.GetUserByEmailAndPassword(loginDto.Email, loginDto.Password);
            if (user == null)
            {
                Log.Warning("Failed login attempt");
                return Unauthorized(new { message = "Invalid email or password" });
            }
            Log.Information("User {Username} logged in successfully.", user.UserName);

            var token = GenerateJwtToken(user);

            var refreshToken = GenerateRefreshToken();
            _refreshTokens[refreshToken.Token] = new RefreshToken
            {
                Token = refreshToken.Token,
                UserId = user.UserId,
                Expiry = refreshToken.Expiry
            };

            return Ok(new 
            { 
                message = "Successfully logged in" ,
                token = token,
                refreshToken = refreshToken.Token
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetRequiredSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("UserId",user.UserId.ToString())
                //new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials : creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expiry = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}
