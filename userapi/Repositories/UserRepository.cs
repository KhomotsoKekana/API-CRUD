using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using UserApi.Context;
using UserApi.Models;
using UserApi.Models.Dto;
using Serilog;

namespace UserApi.Repositories
{
    public interface IUser
    {
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUserById(int id);
        Task<bool> InsertUser(AddUser user);
        Task<bool> UpdateUser(UpdateUser user,int id);
        Task<bool> DeleteUser(int id);
        Task<User> GetUserByUserName(string username);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByEmailAndPassword(string email, string password);
        Task SendEmailAsync(string email, string subject, string message);
  

    }
    public class UserRepository:IUser
    {
        private readonly DapperContext _dapperContext;
   
        public UserRepository(DapperContext dapperContext) 
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var query = "SELECT TOP 200 * FROM dbo.Users";
            using (var connection = _dapperContext.CreateConnection())
            {
                var users = await connection.QueryAsync<User>(query);
                return users;
            }
        }

        public async Task<User> GetUserById(int id)
        {
            var query = "SELECT * FROM dbo.Users WHERE UserId = @UserId";
            using (var connection = _dapperContext.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<User>(query, new { UserId = id });
                return user;
            }
        }

        public async Task<bool> InsertUser(AddUser user)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", user.Name);
            parameters.Add("@Email", user.Email);
            parameters.Add("@UserName", user.UserName);
            parameters.Add("@PhoneNumber", user.PhoneNumber);
            parameters.Add("@Hobbies", user.Hobbies);
            parameters.Add("@IsActive", user.IsActive);

            parameters.Add("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(user.Password));

            var query = @"INSERT INTO dbo.Users (Name,Email,Username,Password,PhoneNumber,CreatedAt,Hobbies,IsActive)
                           VALUES(@Name,@Email,@Username,@PasswordHash,@PhoneNumber,GETDATE(),@Hobbies,@IsActive)";

            using (var connection = _dapperContext.CreateConnection())
            {
                try
                {
                    var result = await connection.ExecuteAsync(query, parameters);
                    return result > 0;
                } catch (SqlException ex)
                {
                    throw new Exception("Database error while inserting user", ex);
                }
            }
        }

        public async Task<bool> UpdateUser(UpdateUser user, int UserId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", user.Name);
            parameters.Add("@Email", user.Email);
            parameters.Add("@UserName", user.UserName);
            parameters.Add("@Hobbies", user.Hobbies);
            parameters.Add("@IsActive", user.IsActive);
            parameters.Add("@UserId", UserId);

            //parameters.Add("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(user.Password));

            var query = @"UPDATE dbo.Users
                          SET
                            Name = @Name,
                            Email = @Email,
                            Username = @UserName,
                            Hobbies = @Hobbies,
                            IsActive = @IsActive
                        WHERE UserId = @UserId";

            using var connection = _dapperContext.CreateConnection();

            var result = await connection.ExecuteAsync(query, parameters) > 0;

            return result;

        }

        public async Task<bool> DeleteUser(int id)
        {
            const string query = "DELETE FROM dbo.Users WHERE UserId = @UserId";

            using var connection = _dapperContext.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { UserId = id }) > 0;

            return result;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var query = "SELECT * FROM dbo.Users WHERE email = @email";
            using (var conn = _dapperContext.CreateConnection())
            {
               var user = await conn.QueryFirstOrDefaultAsync<User>(query, new { email = email });
                return user;
            }
        }

        public async Task<User> GetUserByUserName(string username)
        {
            try
            {
                var query = "SELECT * FROM dbo.Users WHERE UserName = @UserName";
                using (var conn = _dapperContext.CreateConnection())
                {
                    if (conn == null)
                    {
                        throw new InvalidDataException("Database connection failed");
                    }

                    var user = await conn.QueryFirstOrDefaultAsync<User>(query, new { UserName = username });
                    return user;
                }
            }
            catch (Exception ex)
            {

                Log.Error("Error while trying to get user by username", ex.Message);
                return null;
            }
        }

        public async Task<User> GetUserByEmailAndPassword(string email, string password)
        {
            const string query = "SELECT * FROM dbo.Users WHERE email = @Email";
            using (var connection = _dapperContext.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    string subject = "Login Successful";
                    string messageBody = $"Hello {user.UserName},\n\nYou have successfully logged into your account.";

                   // SendEmailAsync(user.Email, subject, messageBody);

                    return user;
                }
                return null;
            }
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            throw new NotImplementedException();
        }

        /*        private string HashPassword(string password)
                {
                    using (var sha256 = SHA256.Create())
                    {
                        Byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                        return Convert.ToBase64String(hashBytes);
                    }
                }*/
    }
}
