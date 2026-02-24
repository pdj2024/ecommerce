using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.JSInterop;
using WebShared.Models;
using PacificWeb.Components;
using PacificWeb.Controllers;
using Polly;
using QuestPDF.Infrastructure;
using Radzen;
using WebShared.Services;
using Microsoft.AspNetCore.HttpOverrides;


QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBlazorBootstrap();
builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
      //.AddInteractiveWebAssemblyComponents();



builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" }
        );
});

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
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
        options.SlidingExpiration = true;
    });

//Production Setting
//options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//options.Cookie.SameSite = SameSiteMode.None;

//Development Setting
//options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//options.Cookie.SameSite = SameSiteMode.Lax;


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



builder.Services.AddHttpClient("ApiClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
}).
ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        AllowAutoRedirect = true
    }).
AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retry =>
            TimeSpan.FromSeconds(Math.Pow(2, retry))
    ));

builder.Services.AddHttpClient<WilayahService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true });

builder.Services.AddSingleton<GlobalDataService>();
builder.Services.AddScoped<ScopedDataServices>();

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();

    // Singleton boleh diambil langsung (tidak menimbulkan scope conflict)
    var globalDataService = sp.GetRequiredService<GlobalDataService>();

    var newpdjservice = sp.GetRequiredService<pdjService>();

    // Scoped boleh juga
    var scopedDataServices = sp.GetRequiredService<ScopedDataServices>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    // Buat instance secara manual
    return new WebShared.Services.NewPdjService(
        globalDataService,
        scopedDataServices,
        jsRuntime,
        navigationManager,
        httpContextAccessor,
        config,
        httpClientFactory);
});
// Register pdjService with a factory method
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var dialogService = sp.GetRequiredService<DialogService>(); // Add this

    var jsRuntime = sp.GetRequiredService<IJSRuntime>(); // Add this

    var authProvider = sp.GetRequiredService<AuthenticationStateProvider>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var iHttpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var config = sp.GetRequiredService<IConfiguration>();


    return new WebShared.Services.pdjService(navigationManager, jsRuntime, authProvider, httpContextAccessor, configuration, iHttpClientFactory); // Pass dialogService and jsRuntime
});


builder.Services.AddScoped(sp =>
{
    var globalDataService = sp.GetRequiredService<GlobalDataService>();

    // Buat instance secara manual
    return new LoginController(globalDataService);
});







builder.Services.AddScoped<Services.TabService>(); // Scoped: unique instance per user session (circuit)


builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddServerSideBlazor()
//    .AddHubOptions(options =>
//    {
//        options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
//        options.KeepAliveInterval = TimeSpan.FromSeconds(10);
//    });

//builder.Services.AddServerSideBlazor()
//    .AddCircuitOptions(options =>
//    {
//        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(3);
//        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);
//        options.MaxBufferedUnacknowledgedRenderBatches = 50;
//        //options.DetailedErrors = true;
//    });

var app = builder.Build();
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();
   //.AddInteractiveWebAssemblyRenderMode();


//app.MapBlazorHub(config =>
//{
//    config.CloseOnAuthenticationExpiration = true;
//}).WithOrder(-1);



app.Run();