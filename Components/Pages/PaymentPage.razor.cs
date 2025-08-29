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
    public partial class PaymentPage 
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

        public List<Pembayaran> ListDataDb { get; set; } = new List<Pembayaran>();
        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<Produk> ListDisplayProduk { get; set; } = new List<Produk>();
        public List<Pembayaran> ListData { get; set; } = new List<Pembayaran>();

        protected override async Task OnInitializedAsync()
        {
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ListDataDb = await pdjService.GetMetodeBayarApi();
                ListDataProduk = await pdjService.GetProdukApi();
                ListDisplayProduk = ListDataProduk.Take(4).ToList();
                ListData.AddRange(ListDataDb);
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("scrollToTop");
                StateHasChanged();
                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("fadeout");
            }

        }

        public async Task NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}");
        }

    }
}
