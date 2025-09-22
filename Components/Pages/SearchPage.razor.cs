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
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Text;
using static Models.Pesanan;
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
        public class searchParam
        {
            public string Keyword { get; set; }
            public string Kategori { get; set; }
            public string Produk { get; set; }
        }

        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<FilterItem> ListFilter { get; set; } = new List<FilterItem>();
        public Produk DataProduk { get; set; } = new Produk();
        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new (), new (), 0, false);
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();
        public searchParam SearchQuery { get; set; } = new searchParam();

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

        }

        private async void OnClearInput()
        {
            SearchText = "";

            SearchList = (new List<Brand>(), new List<Produk>(), 0, false);

            searchCount = 0;

            await SearchDataAsync(SearchText);
            StateHasChanged();
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

            loadProses = true;
            StateHasChanged();

            await SearchDataAsync(SearchText);

            loadProses = false;
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
                var uri = new Uri(NavigationManager.Uri);
                var queryParams = QueryHelpers.ParseQuery(uri.Query);

                if (queryParams.TryGetValue("keyword", out var dataKeyword))
                {
                    SearchQuery.Keyword = Encoding.UTF8.GetString(Convert.FromBase64String(dataKeyword));
                }

                if (queryParams.TryGetValue("produk", out var dataProduk))
                {
                    SearchQuery.Produk = Encoding.UTF8.GetString(Convert.FromBase64String(dataProduk));
                }

                if (queryParams.TryGetValue("kategori", out var dataKategori))
                {
                    SearchQuery.Kategori = Encoding.UTF8.GetString(Convert.FromBase64String(dataKategori));
                }



                SearchText = SearchQuery.Keyword;


                SearchList = await pdjService.GetSearchApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "search",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        keyword = SearchQuery.Keyword,
                        id_kategori = SearchQuery.Kategori,
                        noid_list = SearchQuery.Produk,
                        strict_kategori = false,
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

                if(!string.IsNullOrWhiteSpace(SearchQuery.Kategori))
                {
                    foreach (var idKat in SearchQuery.Kategori.Split(","))
                    {
                        var kate = ListDataKategori.FirstOrDefault(x => x.No_Id == idKat);

                        if (kate != null) 
                        { 
                            ListFilter.Add(new FilterItem { No_Id = kate.No_Id, Nama_Display = kate.Nama_Kategori, Checked = true }); 
                            kate.Checked_Filter = true;
                        }
                    }
                }

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


        private async Task SearchDataAsync(string? query)
        {
            SearchList = await pdjService.GetSearchApi(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "search",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_kategori = string.Join(",", ListFilter.Select(x => x.No_Id)),
                    pagesize = 12,
                    keyword = query,
                    show_variant = true,
                    strict_kategori = true
                }
            });

            searchCount = SearchList.TotalCount == 0 ? -1 : SearchList.TotalCount;

            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initslideProduct");
        }

        private async Task showMore()
        {
            loadProses = true;
            StateHasChanged();

            var NewList = await pdjService.GetSearchApi(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "search",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_kategori = string.Join(",", ListFilter.Select(x => x.No_Id)),
                    pagesize = 12,
                    keyword = SearchText,
                    show_variant = true,
                    cursor_id = SearchList.SearchProduk.LastOrDefault().No_Id,
                    strict_kategori = true
                }
            });

            var data = SearchList.SearchProduk;
            data.AddRange(NewList.SearchProduk);

            SearchList = (NewList.SearchBrand, data, NewList.TotalCount, NewList.hasMore);

            searchCount = SearchList.TotalCount == 0 ? -1 : SearchList.TotalCount;

            loadProses = false;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initslideProduct");
        }
    }
}
