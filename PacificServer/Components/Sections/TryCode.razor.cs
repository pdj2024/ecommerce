using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using WebShared.Models;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using WebShared.Services;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using static Services.pdjService;








namespace PacificWeb.Components.Sections
{
    public partial class TryCode
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        public bool loadProses { get; set; } = false;
        public bool auth { get; set; } = false;


        [Parameter]  public SourchData dataSourch { get; set; } = new();
        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }
        [Parameter] public Produk DataProduk { get; set; } = new Produk();
        [Parameter] public MyHeader header_page { get; set; } = new MyHeader();


        

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            
        }

        public void NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: false);
        }

        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: false);
        }

        private void OnSizeChanged(string value)
        {
            var Size = DataProduk.Sizes.FirstOrDefault(x => x.Size_Id == value);
            DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            DataProduk.Size_Default = Size?.Size_Label ?? "";
        }
        private void OnVariantChanged(string value)
        {
            var Opsi = DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            DataProduk.Option_Default = Opsi?.Option_Label ?? "";
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

        private async Task tambahKeranjang(Produk item)
        {
            if (!auth) 
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");

                await Task.Delay(200);

                await JSRuntime.InvokeVoidAsync("buttonClick", "#buttonLogIn");
                return;
            }

            loadProses = true;
            StateHasChanged();

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity, Id_Customer = dataSourch?.DataUser.No_Id  };
           (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("TAMBAH", itemCart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                loadProses = false;
                await header_page.RefreshCart();
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(200);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#openCartProd");
            } else
            {
                loadProses = false;
                StateHasChanged();
            }
        }
    }
}
