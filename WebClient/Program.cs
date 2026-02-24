using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using WebClient;
using WebShared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<WilayahService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true });

builder.Services.AddSingleton<GlobalDataService>();
builder.Services.AddScoped<ScopedDataServices>();
builder.Services.AddScoped<TabService>();

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
    return new NewPdjService(
        globalDataService,
        scopedDataServices,
        jsRuntime,
        navigationManager,
        httpContextAccessor,
        config,
        httpClientFactory);
});

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
