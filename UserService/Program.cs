using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Steeltoe Cloud Foundry configuration
builder.Configuration.AddCloudFoundry();

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],  // Must match AuthService's issuer
            ValidAudience = builder.Configuration["Jwt:Audience"], // Must match AuthService's audience
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Same signing key as AuthService
        };
    });

// Add Authorization
builder.Services.AddAuthorization(); // <-- Fix for the missing authorization services

// Add Swagger with JWT Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token."
    });

    // Add Security Requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Enable Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI();

// Enable Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Protected Endpoint
app.MapGet("/secure-data", () => "This is secure data.")
    .RequireAuthorization(); // This endpoint requires authentication

app.Run();
