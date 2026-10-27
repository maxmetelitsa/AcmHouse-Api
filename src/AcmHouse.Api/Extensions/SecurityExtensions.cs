using AcmHouse.Api.Security;
using AcmHouse.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AcmHouse.Api.Extensions;

public static class SecurityExtensions
{
    /// <summary>JWT bearer authentication; the signing key comes from configuration (Jwt:Key).</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (jwt.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be at least 32 characters. Set it in appsettings.Development.json, user secrets or the Jwt__Key environment variable.");
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwt.GetSigningKey(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Swagger UI with an "Authorize" button for the JWT token.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AcmHouse API",
                Version = "v1",
                Description = "REST API of the AcmHouse real estate agency: properties, clients, bookings and reports."
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste the accessToken from POST /api/auth/login.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };

            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });

            string xmlFile = Path.Combine(AppContext.BaseDirectory, "AcmHouse.Api.xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }
        });

        return services;
    }
}
