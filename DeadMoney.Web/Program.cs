using DeadMoney.Data;
using DeadMoney.Service.Interfaces;
using DeadMoney.Service.Services;
using DeadMoney.Web.Authorization;
using DeadMoney.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. DATABASE ---
builder.Services.AddDbContextFactory<DeadMoneyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. AUTHENTICATION ---
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Discord";
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.Name = "DeadMoney.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddDiscord("Discord", options =>
{
    var clientId = builder.Configuration["Authentication:Discord:ClientId"];
    var clientSecret = builder.Configuration["Authentication:Discord:ClientSecret"];

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
    {
        throw new InvalidOperationException("Discord ClientId or Secret is missing in secrets.json.");
    }

    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.SaveTokens = true;

    options.Scope.Add("identify");
    options.Scope.Add("email");

    options.Events.OnTicketReceived = async context =>
    {
        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        await userService.ProcessDiscordLoginAsync(context);
    };
});

// Inside the Authorization section
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new CommissionerRequirement()));
});

// Register the handler as a service
builder.Services.AddSingleton<IAuthorizationHandler, CommissionerHandler>();

// --- 3. CUSTOM SERVICES ---
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INflVerseImportService, NflVerseImportService>();

// --- 4. BLAZOR COMPONENTS ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// --- 5. PIPELINE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// --- 5.5 RUNTIME DATABASE INITIALIZATION ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DeadMoneyDbContext>();

    // 1. Ensure migrations are applied (This creates the Roles/Teams/Positions)
    // If you prefer manual migrations, ensure you've run 'Update-Database' in CLI
    await dbContext.Database.MigrateAsync();

    var adminDiscordId = builder.Configuration["AdminSettings:MyDiscordId"];

    if (!string.IsNullOrEmpty(adminDiscordId))
    {
        // 2. Now it's safe to assign the role because MigrateAsync() just created RoleId 1
        await DbInitializer.InitializeUserRoles(dbContext, adminDiscordId);
    }
}

// --- 6. MINIMAL ENDPOINTS ---

app.MapGet("/login", (string returnUrl = "/") =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, ["Discord"]));

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();