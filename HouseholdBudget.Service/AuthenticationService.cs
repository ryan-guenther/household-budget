using Microsoft.AspNetCore.Identity;
using HouseholdBudget.BusinessLogic;
using HouseholdBudget.DTO.Authentication;
using Microsoft.Extensions.Configuration;
using HouseholdBudget.Service.Interfaces;
using Microsoft.Extensions.Logging;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> RegisterAsync(AuthenticationCreateRequestDTO request)
    {
        _logger.LogInformation("Attempting to register a new user with email: {Email}", request.Email);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogError("Registration failed: A user with email {Email} already exists.", request.Email);
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Registration failed for email {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException("Failed to create the user.");
        }

        await _userManager.AddToRoleAsync(user, "User"); // Assign default role

        var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();
        string jwtKey = _configuration["Jwt:Key"];
        string jwtIssuer = _configuration["Jwt:Issuer"];
        string jwtAudience = _configuration["Jwt:Audience"];
        int jwtExpiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"]);

        _logger.LogInformation("Successfully registered user with email: {Email}", user.Email);

        return "Registration successful.";
    }

    public async Task<string> LoginAsync(AuthenticationLoginRequestDTO request)
    {
        _logger.LogInformation("Attempting login for user with email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: No user found with email {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid login attempt.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: Invalid credentials for email {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid login attempt.");
        }

        var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();
        string? jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"];
        string? jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
        string? jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];
        int jwtExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? _configuration["Jwt:ExpiryMinutes"]);

 
        return AuthenticationBL.GenerateJwtToken(
            user.Id,
            user.Email,
            roles,
            jwtKey,
            jwtIssuer,
            jwtAudience,
            jwtExpiryMinutes);
    }
}
