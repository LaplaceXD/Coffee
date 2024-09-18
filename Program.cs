using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Models;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
