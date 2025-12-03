using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
using System.Text;
using static Services.pdjService;








namespace PacificWeb.Components.Sections
{
    public partial class ProductSection
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

        [Inject]
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        public bool loadContent { get; set; } = true;

        [Parameter] public int Quantity { get; set; } = 1;

        [Parameter] public Produk DataProduk { get; set; } = new Produk();

        protected override async Task OnInitializedAsync()
        {
            var bannerTask = NewPdjService.GetListGlobalDataAsync<Produk>(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 12, show_variant = true, max_media = 2, status = "aktif", orderBy = "tgl_created", orderDir = "desc" }
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            await JSRuntime.InvokeVoidAsync("initslideHomeProduct");

        }


        public async Task selectProduk(string id_kode)
        {
            var produkTask = await NewPdjService.GetApi<Produk>(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 2, show_variant = true, max_media = 10, no_id = id_kode }
            });

            
            ScopedDataServices.ScopedData.DataProduk = produkTask.Data.FirstOrDefault(x => x.No_Id == id_kode);

            if (ScopedDataServices.ScopedData.DataProduk == null)
            {
                await JSRuntime.InvokeVoidAsync("initSearchButton");
                StateHasChanged();
                return;
            }

            var dataProd = ScopedDataServices.ScopedData.DataProduk?.Variants.FirstOrDefault(x => x.Stock >= ScopedDataServices.ScopedData.DataProduk?.Min_Order && x.Status.ToLower() == "aktif");

            var Opsi = ScopedDataServices.ScopedData.DataProduk?.Options.FirstOrDefault(x => x.Option_Id == (dataProd?.Option_Id ?? ""));
            ScopedDataServices.ScopedData.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            var Size = ScopedDataServices.ScopedData.DataProduk?.Sizes.FirstOrDefault(x => x.Size_Id == (dataProd?.Size_Id ?? ""));
            ScopedDataServices.ScopedData.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Size_Default = Size?.Size_Label ?? "";

            Quantity = ScopedDataServices.ScopedData.DataProduk?.Min_Order ?? 0;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");

        }
    }
}
