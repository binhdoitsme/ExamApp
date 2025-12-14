using ExamApp.Domain;
using ExamApp.Infrastructure.Database;
using ExamApp.Middlewares;
using ExamApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Google Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new InvalidOperationException("Google ClientId missing");
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new InvalidOperationException("Google ClientSecret missing");

    // Additional scopes
    googleOptions.Scope.Add("profile");
    googleOptions.Scope.Add("email");

    // Map additional claims
    googleOptions.ClaimActions.MapJsonKey("picture", "picture");
    googleOptions.SaveTokens = true;
    googleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
});

// Add services
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("ExamDbConnection")
    ?? throw new InvalidOperationException("Database connection string not found");

builder.Services.AddDbContext<ExamDbContext>(
    options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
builder.Services.AddDbContext<QuestionBankDbContext>(
    options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
builder.Services.AddDbContext<WhitelistDbContext>(
    options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IQuestionBankRepository, QuestionBankRepository>();
builder.Services.AddScoped<IWhitelistRepository, WhitelistRepository>();
builder.Services.AddScoped<ExamService>();
builder.Services.AddScoped<WhitelistService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        RequireHeaderSymmetry = false
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseTokenExpirationCheck();
app.UseAuthorization();

app.MapHealthChecks("/api/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
