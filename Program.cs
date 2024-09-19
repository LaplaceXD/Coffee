using System.Text.Json.Serialization;
using System.Text;

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using ExpenseTrackerAPI.Auth;
using ExpenseTrackerAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddControllers();

builder.Services.AddDbContext<TransactionContext>(opt => opt.UseInMemoryDatabase("Transactions"));
builder.Services.AddDbContext<UserContext>(opt => opt.UseInMemoryDatabase("Users"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ExpenseTrackerAPI",
        Version = "v1",
        Description = "A simple API to track expenses and transactions."
    });

    var filePath = Path.Combine(AppContext.BaseDirectory, "ExpenseTrackerAPI.xml");
    c.IncludeXmlComments(filePath);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Convert enum int values to stringified values
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<JsonOptions>(options =>
{
    // Convert enum int values to stringified values
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>()
        ?? throw new InvalidOperationException("Jwt options are not set in configuration.");

    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.MapControllers();

app.Run();
