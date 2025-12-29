using System.Text;
using Core.Interfaces.Auth;
using Core.Interfaces.Chat;
using Core.Interfaces.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;
using Repository.Repositories.Auth;
using Repository.Repositories.Chat;
using Services.Helpers.Auth;
using Services.Implementations.Auth;
using Services.Implementations.Chat;
using Services.Implementations.Common;

namespace API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<CarFinderDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Auth Repositories
            services.AddScoped<IAuthRepository, AuthRepository>();

            // Chat Repositories
            services.AddScoped<IChatRepository, ChatRepository>();

            // Auth Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IEmailService, EmailService>();

            // Common Services
            services.AddScoped<IFileService, FileService>();

            // Chat Services
            services.AddScoped<IChatService, ChatService>();

            // HttpClient for AI API (ChatService)
            services.AddHttpClient<IChatService, ChatService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "CarFinder-ChatBot/1.0");
            });

            // HttpClient for AI Proxy Controller
            services.AddHttpClient<Controllers.AIProxyController>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "CarFinder-AI-Proxy/1.0");
            });

            // JWT Authentication
            var jwtSecret = configuration["Jwt:Secret"];
            var key = Encoding.ASCII.GetBytes(jwtSecret!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}