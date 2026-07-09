using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

// Friendly landing page so visiting http://localhost:5000/ doesn't 404.
// Ocelot returns 404 for paths it can't match — this handler intercepts
// GET / before Ocelot's pipeline drains the response.
app.Use(async (context, next) =>
{
    if (context.Request.Method == "GET"
        && (context.Request.Path == "/" || context.Request.Path == "/index.html"))
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(
            "RoadReady API Gateway\n" +
            "=====================\n" +
            "This is the Ocelot reverse proxy. All API endpoints live under /api/...\n\n" +
            "Try:\n" +
            "  GET  /health                                  -> AuthService health\n" +
            "  POST /api/v1/auth/login                       -> Login\n" +
            "  GET  /api/v1/cars/search                      -> Browse cars\n" +
            "  GET  /api/v1/bookings/me                      -> My bookings (auth needed)\n\n" +
            "If you reached this page, the gateway is RUNNING. A 404 on a real\n" +
            "endpoint means Ocelot can't match the route — check the spelling.\n");
        return;
    }
    await next();
});

await app.UseOcelot();

app.Run();
