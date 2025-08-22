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
    public partial class DetailProduk 
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
        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }

        [Parameter]
        public string? id { get; set; }   // otomatis bind dari URL

        public string ProductId => id;

        public List<Produk> ListDataProdukApi { get; set; } = new List<Produk>();

        public Produk DataProduk { get; set; } = new Produk();

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Quantity = DataProduk.Min_Order;

                ListDataProdukApi = await pdjService.GetProdukApi(id);
                var data = ListDataProdukApi.FirstOrDefault();
                DataProduk = data != null || ListDataProdukApi.Count >= 1 ? data : new Produk();
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("scrollToTop");
                await JSRuntime.InvokeVoidAsync("statusBarHidden");
                await JSRuntime.InvokeVoidAsync("initDetailProd");
                await JSRuntime.InvokeVoidAsync("initBigDetailProd");
                StateHasChanged();

            }



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
