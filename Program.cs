using Radzen;
using PacificWeb.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Models;
using Microsoft.AspNetCore.Components.Authorization;



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

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "auth_token";
//        options.LoginPath = "/login";
//        options.Cookie.MaxAge = TimeSpan.FromMinutes(10);
//        options.AccessDeniedPath = "/access-denied";
//    });

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








builder.Services.AddScoped<ScreenSizeService>(); // Scoped: New instance per user session

// Register pdjService with a factory method
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var dialogService = sp.GetRequiredService<DialogService>(); // Add this

    var jsRuntime = sp.GetRequiredService<IJSRuntime>(); // Add this

    var authProvider = sp.GetRequiredService<AuthenticationStateProvider>();


    return new Services.pdjService(navigationManager, dialogService, jsRuntime, authProvider); // Pass dialogService and jsRuntime
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