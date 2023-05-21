using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public class AuthRequestBody
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public class CityInfoUser
        {
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? City { get; set; }

            public CityInfoUser(int userId, string? userName, string? firstName, string? lastName, string? password)
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                City = password;
            }
        }

        private IConfiguration _configuration { get; set; }

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthRequestBody authRequestBody)
        {
            var user = ValidateUserCredetials(authRequestBody.Username, authRequestBody.Password);
            if (user == null) return Unauthorized();
         
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            
            // credential used to sign the token, Sha256 - most used algo for signing atm
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // claim = information about who is the user,
            // it contains key-kalue paris, the keys are usually standard
            var claimsForToken = new List<Claim>
            {
                new Claim("sub", user.UserId.ToString()),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName),
                new Claim("city", user.City)
            };
                        
            // info about the token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            // this is the token string we needed
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return tokenToReturn;
        }

        private CityInfoUser ValidateUserCredetials(string? username, string? password)
        {
            return new CityInfoUser(1, 
                username ?? string.Empty, 
                "Antonia", "Grosan", "Baia Mare");
        }

    }
}
