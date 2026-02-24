using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace PacificWeb.Components.Layout
{
    public partial class MainLayout
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        NavigationManager Nav { get; set; }

        bool HideHeaderFooter;
        protected override void OnInitialized()
        {
            Nav.LocationChanged += (_, __) => CheckRoute();
            CheckRoute();
        }

        void CheckRoute()
        {
            var path = Nav.ToBaseRelativePath(Nav.Uri).ToLower();
            // Tentukan halaman yang ingin disembunyikan
            HideHeaderFooter = path.StartsWith("payment-page") || path.StartsWith("login") || path.StartsWith("register") || path.StartsWith("error");
            StateHasChanged();
        }
    }
}
