using EmployeeManagment.Dtos;
using EmployeeManagment.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Exceptions;
using EmployeeManagment_MSSQL.Interfaces;

namespace EmployeeManagment.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthService> logger;
        private readonly IUserRepository userRepository;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, IUserRepository userRepository)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.userRepository = userRepository;
        }

        public string CreateToken(User request)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, request.Role),
                new Claim(ClaimTypes.NameIdentifier, request.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result<User>> RegisterUser(UserRegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return Result<User>.Failure("Username and password are required.");
                }

                request.Password = HashPassword(request.Password);
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = request.Username,
                    PasswordHash = request.Password,
                    Role = request.Role,
                };
                var response = await userRepository.CreateUser(newUser);
                if (!response.IsSuccess)
                {
                    return Result<User>.Failure(response.ErrorMessage);
                }

                return Result<User>.Success(response.Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string HashPassword(string v)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(v)));
        }

        public async Task<Result<string>> LoginUser(UserLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return Result<string>.Failure("Username and password are required.");
                }

                var response = await userRepository.LoginUser(request.Username, request.Password);

                if (!response.IsSuccess) return Result<string>.Failure(response.ErrorMessage);

                var token = CreateToken(response.Value);
                return Result<string>.Success(token);
            }
            catch (Exception e)
            {
                return Result<string>.Failure($"Failed to login user: {e.Message}");
            }
        }

        public async Task<Result<List<User>>> GetAllUsers()
        {
            try
            {
                var response = await userRepository.GetActiveUsers();
                if (!response.IsSuccess) return Result<List<User>>.Failure(response.ErrorMessage);
                return Result<List<User>>.Success(response.Value);
            }
            catch (Exception e)
            {
                return Result<List<User>>.Failure($"Failed to retrieve active users: {e.Message}");
            }
        }

        public async Task<Result> UpdatePassword(string id, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
                {
                    return Result.Failure("User ID and new password are required.");
                }

                var newPassword = HashPassword(password);
                var response = await userRepository.UpdatePassword(id, newPassword);
                if (!response.IsSuccess) return Result.Failure(response.ErrorMessage);
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Failure($"Failed to update password: {e.Message}");
            }
        }
    }
}
