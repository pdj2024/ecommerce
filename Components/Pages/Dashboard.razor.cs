using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using Services;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using static Services.pdjService;








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

        MyHeader header_page;
        MyFooter footer_page;

        public List<Brand> ListDataBanner { get; set; } = new List<Brand>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();
        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public Produk DataProduk { get; set; } = new Produk();
        public bool loadProses { get; set; } = false;

        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ListDataProduk = await pdjService.GetProdukApi(new ParameterApi 
                { routeApi = "produk",
                  prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl 
                    { 
                        pagesize = 8, 
                        show_variant = true,
                    } 
                });

                ListDataBanner = await pdjService.GetBannerApi(new ParameterApi
                {
                    routeApi = "banner",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        orderBy = "nama_banner",
                        orderDir = "desc",
                    }
                });

                ListDataBrand = await pdjService.GetBrandApi(new ParameterApi
                {
                    routeApi = "brand",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        orderBy = "nama_brand",
                        orderDir = "desc",
                    }
                });

                ListDataKategori = await pdjService.GetKategoriApi(new ParameterApi
                {
                    routeApi = "kategori",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                    }
                });

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

        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: true);
        }

        public async Task selectProduk(string id_kode)
        {
            DataProduk = ListDataProduk.FirstOrDefault(x => x.No_Id == id_kode);
            Quantity = 1;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");
        }

        private void OnSizeChanged(string value)
        {
            DataProduk.Size_Default = value;
        }
        private void OnVariantChanged(string value)
        {
            DataProduk.Option = value;
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
            loadProses = true;
            StateHasChanged();

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option == item.Option && x.Size == item.Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity, Id_Customer = "cus_16" };
           (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemCart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                loadProses = false;
                await header_page.RefreshCart();
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(1000);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#openCartProd");
            } else
            {
                loadProses = false;
                StateHasChanged();
            }
        }
    }
}
