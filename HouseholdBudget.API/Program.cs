using HouseholdBudget.Repository;
using HouseholdBudget.Service;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.Repository.Interfaces;
using HouseholdBudget.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(sqlConnectionString));

// Register repositories
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();

// Register services
builder.Services.AddScoped<IExpenseService, ExpenseService>();

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

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    // Configure logging to console
    builder.Logging.AddConsole();

    // Register DbContext with logging enabled
    builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
               .UseLoggerFactory(loggerFactory) // Enables logging
               .EnableSensitiveDataLogging();  // Enables detailed logging (e.g., parameters)
    });

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
