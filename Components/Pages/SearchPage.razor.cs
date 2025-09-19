using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using Services;
using System.Configuration;
using System.Drawing;
using System.Reflection;








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
        public Produk DataProduk { get; set; } = new Produk();
        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount) SearchList { get; set; } = (new (), new (), 0);
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();

        private string SearchText = "";

        private System.Timers.Timer? _debounceTimer;
        [Parameter] public string? Keyword { get; set; }
        [Parameter] public int Quantity { get; set; } = 1;

        MyHeader header_page;
        MyFooter footer_page;

        protected override async Task OnInitializedAsync()
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = QueryHelpers.ParseQuery(uri.Query);

            if (queryParams.TryGetValue("keyword", out var value))
            {
                Keyword = value;
            }

            SearchText = Keyword;


        }

        public async Task selectProduk(string id_kode)
        {
            DataProduk = SearchList.SearchProduk.FirstOrDefault(x => x.No_Id == id_kode);
            Quantity = 1;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");
        }

        public void NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: true);
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                SearchList = await pdjService.GetSearchApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "search",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        keyword = Keyword,
                        pagesize = 12,
                        show_variant = true,
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



                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("scrollToTop");
                await JSRuntime.InvokeVoidAsync("initslideProduct");
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
