using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Core.Services.Contact;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GradingManagementSystem.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly SymmetricSecurityKey _Key;

        public TokenService(IConfiguration configuration, UserManager<AppUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? string.Empty));
        }

        public async Task<string> CreateTokenAsync(AppUser user)
        {            
            var AuthClaims = new List<Claim>()
            {
                new Claim("UserId", user.Id),
                new Claim("UserName", user.FullName),
                new Claim("UserEmail", user.Email ?? string.Empty),
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                AuthClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            var credential = new SigningCredentials(_Key, SecurityAlgorithms.HmacSha256);

            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:Issuer"],
                SigningCredentials = credential,
                Subject = new ClaimsIdentity(AuthClaims),
                Expires = DateTime.Now.AddDays(15),
                IssuedAt = DateTime.Now,
            };

            var tokenhandler = new JwtSecurityTokenHandler();

            var token = tokenhandler.CreateToken(TokenDescriptor);

            return tokenhandler.WriteToken(token);
        }
    }
}
