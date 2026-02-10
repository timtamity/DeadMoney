using DeadMoney.Core.Entities; // Global priority for ApplicationUser
using DeadMoney.Data;
using DeadMoney.Services.Interfaces;
using DeadMoney.Services.Sleeper;
using DeadMoney.Web.Components;
using DeadMoney.Web.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<DeadMoneyDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. IDENTITY
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
        options.Scope.Add("identify");
        options.Scope.Add("email");
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    })
    .AddIdentityCookies();

// This line MUST use the ApplicationUser from DeadMoney.Core.Entities
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DeadMoneyDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// This now works because both the interface and the implementation 
// are using the exact same ApplicationUser type from Core.Entities.
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// 3. CUSTOM SERVICES
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPlayerSyncService, SleeperPlayerSyncService>();

// 4. BLAZOR COMPONENTS
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();