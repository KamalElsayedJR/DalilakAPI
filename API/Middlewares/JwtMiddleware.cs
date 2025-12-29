using System.IdentityModel.Tokens.Jwt;
using Core.Interfaces.Auth;

namespace API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                // Resolve IJwtService from the request scope
                var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
                AttachUserToContext(context, token, jwtService);
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token, IJwtService jwtService)
        {
            try
            {
                var principal = jwtService.GetClaimsFromToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
            }
            catch
            {
                // Token validation failed, do nothing
            }
        }
    }
}