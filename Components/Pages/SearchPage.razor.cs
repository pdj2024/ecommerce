using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using Radzen.Blazor;
using Radzen;
using Services;
using System.Reflection;

using System.Configuration;

using System.Drawing;








namespace PacificWeb.Components.Pages
{
    public partial class SearchPage 
    {

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();

        private string SearchText = "";

        private System.Timers.Timer? _debounceTimer;

        protected override async Task OnInitializedAsync()
        {
            ListDataProduk = await pdjService.GetProdukApi();
            ListDataKategori = await pdjService.GetKategoriApi();
            ListDataBrand = await pdjService.GetBrandApi();
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // await JSRuntime.InvokeVoidAsync("initTinySlider");
                await JSRuntime.InvokeVoidAsync("scrollToTop");
                // await JSRuntime.InvokeVoidAsync("statusBarHidden");
            }

        }

        private void OnSearchInput(string? value)
        {
            SearchText = value;

            // reset timer
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            _debounceTimer = new System.Timers.Timer(500); // 500ms delay
            _debounceTimer.Elapsed += async (s, e) =>
            {
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();

                await InvokeAsync(async () =>
                {
                    await SearchDataAsync(SearchText);
                    StateHasChanged();
                });
            };
            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();
        }

        private void OnClearInput()
        {
            SearchText = "";
        }

        private Task SearchDataAsync(string? query)
        {
            Console.WriteLine($"🔎 Search dipanggil: {query}");
            return Task.CompletedTask;
        }
    }
}
