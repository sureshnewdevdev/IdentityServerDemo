// this is just authendticatio server we are creating it for token based autendtication
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);

// Add IdentityServer
builder.Services.AddIdentityServer()
    .AddInMemoryClients(new List<Client>
    {
        new Client
        {
            ClientId = "test-client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { "api.read" }
        }
    })
    .AddInMemoryApiScopes(new List<ApiScope>
    {
        new ApiScope("api.read", "Read Access to API")
    })
    .AddInMemoryIdentityResources(new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    })
    .AddTestUsers(new List<TestUser>
    {
        new TestUser
        {
            SubjectId = "1",
            Username = "test-user",
            Password = "password",
            Claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("name", "Test User"),
                new System.Security.Claims.Claim("email", "test-user@example.com")
            }
        }
    });

var app = builder.Build();
//app.UseHttpsRedirection( (false);
app.MapGet("/", () => "Hello World!");
app.UseIdentityServer();
app.Run();
