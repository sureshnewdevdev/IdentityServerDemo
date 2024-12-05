using Microsoft.AspNetCore.Mvc;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add Steeltoe Cloud Foundry configuration
builder.Configuration.AddCloudFoundry();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for communicating with IdentityServer
builder.Services.AddHttpClient("IdentityServer", client =>
{
    client.BaseAddress = new Uri("https://localhost:7065"); // Replace with IdentityServer URL
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// Endpoint to get token from IdentityServer
app.MapPost("/token", async ([FromServices] IHttpClientFactory httpClientFactory, [FromBody] LoginRequest request) =>
{
    if (request.ClientId == "test-client" && request.ClientSecret == "secret")
    {
        // Call IdentityServer's /connect/token endpoint
        var client = httpClientFactory.CreateClient("IdentityServer");
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", request.ClientId),
            new KeyValuePair<string, string>("client_secret", request.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "api.read")
        });

        var response = await client.PostAsync("/connect/token", formData);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return Results.Ok(responseContent);
        }

        return Results.StatusCode((int)response.StatusCode);
    }

    return Results.Unauthorized();
})
.WithName("GetToken");

app.Run();

public record LoginRequest(string ClientId, string ClientSecret);
