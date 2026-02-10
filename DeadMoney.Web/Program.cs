using System.Security.Claims;
using DeadMoney.Core.Entities;
using DeadMoney.Data;
using DeadMoney.Services.Interfaces;
using DeadMoney.Services.Sleeper;
using DeadMoney.Web.Components;
using DeadMoney.Web.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<DeadMoneyDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. IDENTITY & AUTHENTICATION
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider>();

builder.Services.AddScoped<UserProvisioningService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;

        options.Events.OnTicketReceived = async context =>
        {
            var provisioner = context.HttpContext.RequestServices.GetRequiredService<UserProvisioningService>();
            await provisioner.ProvisionUserAsync(context.Principal!);

            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

            var discordId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.DiscordId == discordId);

            if (user != null)
            {
                await signInManager.SignInAsync(user, isPersistent: true);
            }
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

        options.Events.OnTicketReceived = async context =>
        {
            var provisioner = context.HttpContext.RequestServices.GetRequiredService<UserProvisioningService>();
            await provisioner.ProvisionUserAsync(context.Principal!);

            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

            var googleId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.DiscordId == googleId);

            if (user != null)
            {
                await signInManager.SignInAsync(user, isPersistent: true);
            }
        };
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DeadMoneyDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/login";
});

// 3. CUSTOM SERVICES & MVC SUPPORT
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPlayerSyncService, SleeperPlayerSyncService>();

// Added to support traditional MVC Controllers (like PlayersController)
builder.Services.AddControllersWithViews();

// 4. BLAZOR COMPONENTS
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 1024 * 1024);

var app = builder.Build();

// 5. PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// 6. ENDPOINTS

// MVC Controller Route Mapping (This resolves /Players)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Minimal Auth Endpoints
app.MapPost("/Account/PerformExternalLogin", (
    [FromForm] string provider,
    [FromForm] string returnUrl) =>
{
    var properties = new AuthenticationProperties { RedirectUri = returnUrl };
    return Results.Challenge(properties, [provider]);
});

app.MapPost("/Account/Logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
});

// Blazor Mapping
app.MapRazorComponents<DeadMoney.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();