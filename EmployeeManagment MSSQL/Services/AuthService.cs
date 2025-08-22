using EmployeeManagment.Dtos;
using EmployeeManagment.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagment_MSSQL.Interfaces;
using User = EmployeeManagment.Models.User;

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

        public async Task<User> RegisterUser(UserRegisterRequest request)
        {
            request.Password = HashPassword(request.Password);
            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                PasswordHash = request.Password,
                Role = request.Role,
            };
            var response = await userRepository.CreateUser(newUser);
            return response;
        }

        private string HashPassword(string v)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(v)));
        }

        public async Task<string> LoginUser(UserLoginRequest request)
        {
            var response = await userRepository.LoginUser(request.Username, request.Password);

            if(response == null) return null;

            var token = CreateToken(response);
            return token;
        }

        public async Task<List<User>> GetAllUsers()
        {
           var response = await userRepository.GetActiveUsers();
           return response;
        }

        public async Task<bool> UpdatePassword(string id, string password)
        {
            var newPassword = HashPassword(password);
            var response = await userRepository.UpdatePassword(id, newPassword);
           return response;
        }
    }
}
