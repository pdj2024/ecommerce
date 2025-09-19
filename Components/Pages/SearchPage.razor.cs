using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json.Linq;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using Services;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;








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

        public class FilterItem
        {
            public string Nama_Display { get; set; }
            public string No_Id { get; set; }
            public bool Checked { get; set; }
        }

        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<FilterItem> ListFilter { get; set; } = new List<FilterItem>();
        public Produk DataProduk { get; set; } = new Produk();
        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount) SearchList { get; set; } = (new (), new (), 0);
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();

        private string SearchText = "";

        private System.Timers.Timer? _debounceTimer;
        [Parameter] public string? Keyword { get; set; }
        [Parameter] public int Quantity { get; set; } = 1;

        public int searchCount { get; set; } = 0;
        public bool loadProses { get; set; } = false;

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

        public async Task removeAllFilter()
        {

            foreach (var filter in ListFilter)
            {
                var Item = ListDataKategori.FirstOrDefault(x => x.No_Id == filter.No_Id && x.Nama_Kategori == filter.Nama_Display);
                if (Item != null)
                {
                    Item.Checked_Filter = false;
                }

                var Item1 = ListDataBrand.FirstOrDefault(x => x.No_Id == filter.No_Id && x.Nama_Brand == filter.Nama_Display);
                if (Item1 != null)
                {
                    Item1.Checked_Filter = false;
                }
            }

            ListFilter.Clear();
            StateHasChanged();
        }

        public async Task changeFilter(Kategori dataKategori = null, Brand dataBrand = null, ChangeEventArgs value = null, MouseEventArgs valueClick = null, FilterItem dataFilter = null)
        {
            if (dataKategori != null)
            {
                var Item = ListDataKategori.FirstOrDefault( x => x.No_Id == dataKategori.No_Id);
                var Data = ListFilter.FirstOrDefault(x => x.No_Id == Item.No_Id && x.Nama_Display == Item.Nama_Kategori);

                if (Data == null) ListFilter.Add(new FilterItem { No_Id = Item.No_Id, Nama_Display = Item.Nama_Kategori, Checked = true });
                else ListFilter.Remove(Data);


                if (value != null)
                {
                    Item.Checked_Filter = (bool)value.Value;

                }
                else if (valueClick != null)
                {
                    Item.Checked_Filter = false;
                }
            } 
            
            else if (dataBrand != null)
            {
                var Item = ListDataBrand.FirstOrDefault(x => x.No_Id == dataBrand.No_Id);
                var Data = ListFilter.FirstOrDefault(x => x.No_Id == Item.No_Id && x.Nama_Display == Item.Nama_Brand);

                if (Data == null) ListFilter.Add(new FilterItem { No_Id = Item.No_Id, Nama_Display = Item.Nama_Brand, Checked = true });
                else ListFilter.Remove(Data);


                if (value != null)
                {
                    Item.Checked_Filter = (bool)value.Value;

                }
                else if (valueClick != null)
                {
                    Item.Checked_Filter = false;
                }

            } 
            
            else
            {
                var ItemBrand = ListDataBrand.FirstOrDefault(x => x.No_Id == dataFilter.No_Id);
                var ItemKategori = ListDataKategori.FirstOrDefault( x => x.No_Id == dataFilter.No_Id);

                if (ItemBrand != null)
                {
                    ItemBrand.Checked_Filter = false;
                    ListFilter.Remove(dataFilter);
                }

                if (ItemKategori != null)
                {
                    ItemKategori.Checked_Filter = false;
                    ListFilter.Remove(dataFilter);
                }
            }

            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            _debounceTimer = new System.Timers.Timer(500); // 500ms delay

            _debounceTimer.Elapsed += async (s, e) =>
            {
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();

                await InvokeAsync(async () =>
                {
                    loadProses = true;
                    StateHasChanged();

                    await SearchDataAsync(SearchText);

                    loadProses = false;
                    StateHasChanged();
                });
            };

            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();

            StateHasChanged();
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
                    loadProses = true;
                    StateHasChanged();

                    await SearchDataAsync(SearchText);

                    loadProses = false;
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


        private async Task SearchDataAsync(string? query)
        {
            SearchList = await pdjService.GetSearchApi(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "search",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_kategori = string.Join(",", ListFilter.Select(x => x.No_Id)),
                    pagesize = 8,
                    keyword = query
                }
            });

            searchCount = SearchList.TotalCount == 0 ? -1 : SearchList.TotalCount;

            StateHasChanged();
        }
    }
}
