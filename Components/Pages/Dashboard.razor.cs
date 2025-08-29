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
    public partial class Dashboard
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
        public List<Brand> ListDataBanner { get; set; } = new List<Brand>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public Produk DataProduk { get; set; } = new Produk();

        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ListDataProduk = await pdjService.GetProdukApi();
                ListDataBanner = await pdjService.GetBannerApi();
                ListDataBrand = await pdjService.GetBrandApi();
                ListDataKategori = await pdjService.GetKategoriApi();
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initslideProduct");
                await JSRuntime.InvokeVoidAsync("initTinySlider");
                await JSRuntime.InvokeVoidAsync("scrollToTop");
                //await JSRuntime.InvokeVoidAsync("statusBarHidden");
                StateHasChanged();
                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("fadeout");
            }
        }

        public void NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: true);
        }

        public async void selectProduk(string id_kode)
        {
            DataProduk = ListDataProduk.FirstOrDefault(x => x.No_Id == id_kode);
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");
        }

        private void OnSizeChanged(string value)
        {
            DataProduk.Size_Default = value;
        }

        public class SizeOption
        {
            public string Value { get; set; }
            public bool Disabled { get; set; }
            public SizeOption(string value, bool disabled = false)
            {
                Value = value;
                Disabled = disabled;
            }
        }

        private async Task Increase()
        {
            Quantity++;
            await QuantityChanged.InvokeAsync(Quantity);
        }

        private async Task Decrease()
        {
            if (Quantity > 1)
            {
                Quantity--;
                await QuantityChanged.InvokeAsync(Quantity);
            }
        }
    }
}
