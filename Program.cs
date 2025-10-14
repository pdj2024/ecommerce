using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Controllers;
using PacificWeb.Components;
using Radzen;
using Services;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBlazorBootstrap();
builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);



builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" }
        );
}
);

builder.Services.AddAuthorization(config =>
{
    foreach (var userPolicy in UserPolicy.GetPolicies()) // all actions
    {
        config.AddPolicy(userPolicy, cfg => cfg.RequireClaim(userPolicy, "true"));
    }
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/access-denied";
        options.LoginPath = "/login";
        options.Cookie.Name = "auth_token";
        options.Cookie.MaxAge = TimeSpan.FromDays(30);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
        options.SlidingExpiration = true;
    });

builder.Services.AddCascadingAuthenticationState();

//builder.Services.AddDevExpressBlazor();
//builder.Services.AddDevExpressServerSideBlazorReportViewer();
//// If you use Bootstrap 5, specify the Bootstrap version explicitly
//builder.Services.Configure<DevExpress.Blazor.Configuration.GlobalOptions>(options => {
//    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
//});


builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();


builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<ScrollService>();
builder.Services.AddScoped<LoginService>();







builder.Services.AddScoped<ScreenSizeService>(); // Scoped: New instance per user session

// Register pdjService with a factory method
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var dialogService = sp.GetRequiredService<DialogService>(); // Add this

    var jsRuntime = sp.GetRequiredService<IJSRuntime>(); // Add this

    var authProvider = sp.GetRequiredService<AuthenticationStateProvider>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();


    return new Services.pdjService(navigationManager, dialogService, jsRuntime, authProvider, httpContextAccessor); // Pass dialogService and jsRuntime
});

builder.Services.AddScoped(sp =>
{

    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var jsRuntime = sp.GetRequiredService<IJSRuntime>(); // Add this
    var authProvider = sp.GetRequiredService<AuthenticationStateProvider>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

    return new PacificWeb.Controllers.LoginController(navigationManager, jsRuntime, authProvider, httpContextAccessor); // Pass dialogService and jsRuntime
});






builder.Services.AddScoped<Services.TabService>(); // Scoped: unique instance per user session (circuit)


builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();


app.MapBlazorHub(config =>
{
    config.CloseOnAuthenticationExpiration = true;
}).WithOrder(-1);



app.Run();