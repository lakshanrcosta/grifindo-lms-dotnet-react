using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace grifindo_lms_api.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Cookies["auth_token"];

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Fetch secret and issuer from environment variables
                    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                        ?? throw new ArgumentNullException("Environment variable 'JWT_SECRET_KEY' is not set.");
                    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? throw new ArgumentNullException("Environment variable 'JWT_ISSUER' is not set.");

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(secretKey);

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = issuer,
                        ValidAudience = issuer,
                        ValidateLifetime = true
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;

                    // Attach user information to the context (optional)
                    var claims = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));
                    context.User = claims;
                }
                catch
                {
                    // Invalid token; clear context
                    context.User = new ClaimsPrincipal();
                }
            }

            await _next(context);
        }
    }
}
