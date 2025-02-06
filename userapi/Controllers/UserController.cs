using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using UserApi.Models;
using UserApi.Models.Dto;
using UserApi.Repositories;

namespace UserApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUser _userInterface;
        private readonly UserRepository _userRepo;

        public UserController(IConfiguration configuration, IUser user)
        {
            _configuration = configuration;
            _userInterface = user;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            try
            {
                var user = await _userInterface.GetUserById(id);

                if (user == null)
                {
                    return NotFound(new { message = $"User with id {id} not found" });
                }

                return Ok(user);
            }
            catch (Exception e)
            {

                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userInterface.GetUsers();

                if (users != null)
                {
                    return Ok(users);
                }
                else
                {
                    return BadRequest(new { message = "No users found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to retrieve users", details = ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> InsertUser([FromBody] AddUser user)
        {
            if (user == null)
            {
                return BadRequest(new { MESSAGE = "User cannot be null" });
            }

            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email))
            {
                return BadRequest(new { MESSAGE = "Name and Email are required" });
            }

            try
            {
                var success = await _userInterface.InsertUser(user);
                if (success)
                {
                    Log.Information("User created successfully");
                    return Ok(new { message = "User created successfully" });
                } else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occured while creating the user" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsers([FromBody] UpdateUser user,int id)
        {
            if (user == null)
            {
                return BadRequest(new { message = "User data cannot be null" });
            }

            var existingUser = await _userInterface.GetUserById(id);

            if (existingUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            try
            {
                await _userInterface.UpdateUser(user,id);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occured while updating the user" });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute, Required] int id)
        {
            try
            {
                var result = await _userInterface.DeleteUser(id);
                if (result)
                    return Ok(new { message = "User successfully deleted" });
                else
                    return NotFound( new {message = ""});

            } catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occured while deleting the user" });
            }
        }
    }
}
