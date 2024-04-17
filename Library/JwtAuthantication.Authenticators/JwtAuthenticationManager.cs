using JwtAuthentication.Catalog.User;
using JwtAuthentication.Common;
using JwtAuthentication.Models.User;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Authenticators {
    public class JwtAuthenticationManager {

        private readonly string _key = "";
        private readonly string _issuer = "";
        private readonly string _audience = "";

        private IConfiguration _configuration;
        private IRepository<User> _userRepository;

        public JwtAuthenticationManager(IConfiguration configuration,IRepository<User> userRepository) {
            _configuration = configuration;
            _userRepository = userRepository;

            _key = _configuration["Jwt:Key"]!;
            _issuer = _configuration["Jwt:Issuer"]!;
            _audience = _configuration["Jwt:Audience"]!;
        }

        public UserModel GetCurrentUser(ClaimsPrincipal identity) {
            if(identity == null)
                return null;

            var claims = identity.Claims;

            bool hasId = claims.Any(o => o.Type == "DataId");

            if(!hasId)
                return null;

            return new UserModel() {
                Claims = claims.Select(o => o.Value).ToArray(),
                Id = Guid.Parse(claims.FirstOrDefault(o => o.Type == "DataId")?.Value),
                Username = claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value
            };
        }

        public async Task<User> Authenticate(User user) {
            user.AccessToken = GenerateToken(user,out DateTime expiry);
            user.TokenExpiry = expiry;
            user.RefreshToken = Guid.NewGuid();

            return user;
        }

        public async Task<AuthenticationResult> GenerateTokenFromRefresh(AuthenticationResult oldToken) {

            var result = await IsValid(oldToken);
            if(!result.IsValid)
                return null;

            string token = GenerateToken(result.User,out DateTime expiry);

            return new AuthenticationResult() {
                Expiry = expiry,
                RefreshToken = Guid.NewGuid(),
                Token = token
            };
        }

        private async Task<RefreshTokenResult> IsValid(AuthenticationResult oldToken) {
            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(oldToken.Token);
            if(principal == null)
                throw new UnauthorizedAccessException("Principal is empty");  // Somebody is trying something.

            var principalUsername = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // check if we have the refresh token in the system.
            User dbUser = await _userRepository.GetAsync(o => o.Phone == principalUsername);

            if(dbUser == null)
                throw new UnauthorizedAccessException("No user with that name"); // Somebody is trying something.

            if(dbUser.RefreshToken != oldToken.RefreshToken)
                throw new UnauthorizedAccessException("Refresh token does not belong to anybody.");

            return new RefreshTokenResult() { IsValid = true,UserName = principalUsername,User = dbUser };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token) {
            var tokenValidation = new TokenValidationParameters() {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimTypes.Role
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token,tokenValidation,out SecurityToken securityToken);
            if(securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,StringComparison.InvariantCulture)) {
                throw new SecurityTokenException("Invalid Token");
            }

            return principal;
        }

        public string GenerateToken(User user,out DateTime expiry) {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            string nameIdentifier = "";
            if(!string.IsNullOrEmpty(user.Phone))
                nameIdentifier = user.Phone;

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                new Claim("DataId", user.Id.ToString()),
            };

            foreach(var role in user.Roles) {
                claims.Add(new Claim(ClaimTypes.Role,role));
            }

            expiry = DateTime.UtcNow.AddHours(24);

            var token = new JwtSecurityToken(_issuer,_audience,claims,
                expires: expiry,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
