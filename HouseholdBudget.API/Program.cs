using HouseholdBudget.Repository;
using HouseholdBudget.Service;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.Infrastructure;
using DotNetEnv;
using HouseholdBudget.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file (only in Development)
if (builder.Environment.IsDevelopment())
{
    // Load environment variables from .env file
    Env.Load();
}

var sqlConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Configure logging to console, enabling detailed logging in Development mode
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole(options =>
    {
        options.IncludeScopes = true; // Optionally add scopes for better logging in Development
    });
}

// Add DbContext and conditionally enable sensitive data logging in Development mode
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    options.UseSqlServer(sqlConnectionString)
           .UseLoggerFactory(loggerFactory);  // Enables logging

    // Enable sensitive data logging only in Development mode
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();  // Enables detailed logging (e.g., parameters)
    }
});

// Register repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Mapster configuration
builder.Services.AddSingleton(MapsterConfig.Configure());

// Register services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();


// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// Add controllers
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger/OpenAPI for Development and Staging environments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline for Development or Staging environments
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Global exception handling
app.UseExceptionHandler("/error");

app.MapControllers();

app.Run();
