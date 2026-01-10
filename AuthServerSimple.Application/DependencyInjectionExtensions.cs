using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Application.Options;
using AuthServerSimple.Application.Services;
using AuthServerSimple.Application.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServerSimple.Application;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddApplicationLayerDependencies(this WebApplicationBuilder builder)
    {
        // Options
        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection(JwtOptions.JwtOptionsSectionName));
        builder.Services.Configure<SeedOptions>(
            builder.Configuration.GetSection(SeedOptions.SeedOptionsSectionName));

        // Services
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Validation
        builder.Services.AddValidatorsFromAssemblyContaining<CreateRoleRequestValidator>();
        
        return builder;
    }
}