using CitiesManager.Core.Domain.Identity;
using CitiesManager.Core.DTO;
using CitiesManager.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CitiesManager.Core.Service
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthResponseDto CreateJwtToken(ApplicationUser user)
        {
            DateTime? expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:TokenExpiryMins"]));
            // claims are the details need to added to payload.
            Claim[] claims = new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),// User Id
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),// new jwt id for a token
                new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),// Issueed at.
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.PersonName.ToString()),
                new Claim(ClaimTypes.Email,user.Email.ToString())
                // like above we can add addition details of user other that user id
                // using ClaimTypes.

            };

            // to generate signature we need secretkey
            SymmetricSecurityKey? SecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            // using "Jwt:SecretKey" we can create a SecretKey that  will ne hash with 
            // header and payload to generate sinature.
            SigningCredentials credentials = new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha256);
            // here we mention SecretKey and name of algorithm for hasing.

            // tokenGenerator is responsible for generating token.
            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
               _configuration["Jwt:Issuer"],
               _configuration["Jwt:Audience"],
               claims,
               expires: expires,
               signingCredentials: credentials
               );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string token =tokenHandler.WriteToken(tokenGenerator);

            return new AuthResponseDto { 
                Token = token,
                Email=user.Email,
                PersonName=user.PersonName,
                Expiration= expires,
                RefreshToken= GenerateRefreshToken(),
                RefreshTokenExpiration=DateTime.UtcNow.AddMinutes(Convert.ToDouble( _configuration["RefreshToken:TokenExpiryMins"])),
            };   

        }

        public ClaimsPrincipal? GetClaimsPrincipalFromToken(string token)
        {
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience =_configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                // Above code says we have to validate Audience and Issuer with above values.
                ValidateLifetime = false,// it should be false coz we expecting its already expired.
                ValidateIssuerSigningKey = true,//its the signature part in token
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]))

            };
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            ClaimsPrincipal? claimsPrincipal = jwtSecurityTokenHandler.ValidateToken(token,tokenValidationParameters,out SecurityToken securityToken);
            // if securityToken is not null then token is valid.

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                //if securityToken is not type of JwtSecurityToken or
                // algorithm is not HmacSha256 then
                throw new SecurityTokenException("Invalid Token");

            }
            return claimsPrincipal;
        }

        private string GenerateRefreshToken()
        {
            byte[] bytes= new byte[64];
            var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
