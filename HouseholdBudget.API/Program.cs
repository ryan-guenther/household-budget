using HouseholdBudget.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
if (builder.Environment.IsDevelopment())
    DotNetEnv.Env.Load();

// Apply configurations
builder.Services.AddLoggingConfiguration(builder.Environment);
builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddIdentityConfiguration();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddServiceConfiguration();
builder.Services.AddControllersConfiguration();

var app = builder.Build();

// Middleware configuration
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler("/error");

app.MapControllers();

app.Run();
