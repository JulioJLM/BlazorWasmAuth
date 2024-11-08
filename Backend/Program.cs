using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend;
using Backend.Data;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// establish cookie authentication
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

// Configure app cookie
//
// The default values, which are appropriate for hosting the Backend and
// BlazorWasmAuth apps on the same domain, are Lax and SameAsRequest. 
// For more information on these settings, see:
// https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#cross-domain-hosting-same-site-configuration
/*
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
*/

// configure authorization
builder.Services.AddAuthorizationBuilder();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// add identity and opt-in to endpoints
builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

// add a CORS policy for the client
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:5001", 
            builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// add services to the container
builder.Services.AddEndpointsApiExplorer();

// add NSwag services
builder.Services.AddOpenApiDocument();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    // seed the database
    await using var scope = app.Services.CreateAsyncScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);

    // add OpenAPI/Swagger generator and the Swagger UI
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// create routes for the identity endpoints
app.MapIdentityApi<AppUser>();

// activate the CORS policy
app.UseCors("wasm");

// Enable authentication and authorization after CORS Middleware
// processing (UseCors) in case the Authorization Middleware tries
// to initiate a challenge before the CORS Middleware has a chance
// to set the appropriate headers.
app.UseAuthentication();
app.UseAuthorization();

// provide an endpoint to clear the cookie for logout
//
// For more information on the logout endpoint and antiforgery, see:
// https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
app.MapPost("/logout", async (SignInManager<AppUser> signInManager, [FromBody] object empty) =>
{
    if (empty is not null)
    {
        await signInManager.SignOutAsync();

        return Results.Ok();
    }

    return Results.Unauthorized();
}).RequireAuthorization();

app.UseHttpsRedirection();

// provide an endpoint for user roles
app.MapGet("/roles", (ClaimsPrincipal user) =>
{
    if (user.Identity is not null && user.Identity.IsAuthenticated)
    {
        var identity = (ClaimsIdentity)user.Identity;
        var roles = identity.FindAll(identity.RoleClaimType)
            .Select(c => 
                new
                {
                    c.Issuer, 
                    c.OriginalIssuer, 
                    c.Type, 
                    c.Value, 
                    c.ValueType
                });

        return TypedResults.Json(roles);
    }

    return Results.Unauthorized();
}).RequireAuthorization();

// Set up API endpoints and methods
var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/all/{take}/{id}", TodoService.GetAllTodos).RequireAuthorization();
todoItems.MapGet("/complete", TodoService.GetCompleteTodos).RequireAuthorization();
todoItems.MapGet("/{id}", TodoService.GetTodo).RequireAuthorization();
todoItems.MapPost("/", TodoService.CreateTodo).RequireAuthorization();
todoItems.MapPut("/{id}", TodoService.UpdateTodo).RequireAuthorization();
todoItems.MapDelete("/{id}", TodoService.DeleteTodo).RequireAuthorization();

app.Run();

// example form model
class FormModel
{
    public string Message { get; set; } = string.Empty;
}
