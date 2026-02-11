using DeadMoney.Data;
using DeadMoney.Service.Interfaces;
using DeadMoney.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DeadMoney.Web.Service.Interfaces;
using DeadMoney.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE
builder.Services.AddDbContextFactory<DeadMoneyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. AUTHENTICATION (Manual Cookies + Discord)
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
})
.AddDiscord("Discord", options =>
{
    options.ClientId = builder.Configuration["Discord:ClientId"]!;
    options.ClientSecret = builder.Configuration["Discord:ClientSecret"]!;
    options.SaveTokens = true;

    options.Events.OnTicketReceived = async context =>
    {
        // Use our custom service to find/create the user and attach roles
        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        await userService.ProcessDiscordLoginAsync(context);
    };
});

builder.Services.AddAuthorization();

// 3. CUSTOM SERVICES
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUserService, UserService>(); // Your new bridge service
builder.Services.AddScoped<INflVerseImportService, NflVerseImportService>();

// 4. BLAZOR COMPONENTS
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// 5. PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// 6. MINIMAL ENDPOINTS (Replaces MVC Controllers)
app.MapGet("/login", (string returnUrl = "/") =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, ["Discord"]));

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();