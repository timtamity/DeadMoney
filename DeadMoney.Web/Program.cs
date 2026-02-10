using DeadMoney.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in secrets.json.");

builder.Services.AddDbContext<DeadMoneyDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("DeadMoney.Data")));

// 2. Identity Core Setup (Optimized for OAuth - No Local Passwords)
builder.Services.AddIdentityCore<IdentityUser>(options => {
    // Since we use OAuth, Discord/Google verify the email for us
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<DeadMoneyDbContext>()
.AddSignInManager<SignInManager<IdentityUser>>()
.AddDefaultTokenProviders();

// 3. Authentication & OAuth (Discord & Google)
builder.Services.AddAuthentication(options => {
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options => {
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
})
.AddCookie(IdentityConstants.ExternalScheme)
.AddDiscord(options => {
    options.ClientId = builder.Configuration["Authentication:Discord:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"] ?? "";
    options.SaveTokens = true;
})
.AddGoogle(options => {
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
});

// 4. Blazor Component Services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Support for Authorization attributes
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// 5. Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Maps Blazor components and sets the Render Mode
app.MapRazorComponents<DeadMoney.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();