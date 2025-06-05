using Microsoft.Extensions.DependencyInjection;
using Common.Modularity;
using Common.Modularity.Attributes;
using Application.Contracts.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Domain.Security.Interfaces;
using Infrastructure.Security;
using Domain;
using Application.Contracts;
using Common;

namespace Infrastructure;

[DependsOn(
    typeof(DomainModule),
    typeof(ApplicationContractsModule),
    typeof(CommonModule)
    )]
public class InfrastructureModule : Module
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IDatabaseService, PostgresDatabaseService>();

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = context.Configuration["Jwt:Issuer"],
                    ValidAudience = context.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(context.Configuration["Jwt:SecretKey"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        context.Services.AddSingleton<IJwtGenerator, JwtGenerator>();
    }
}
