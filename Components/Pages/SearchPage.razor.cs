using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        [Inject]
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

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
            public string Brand { get; set; }
        }

        public List<FilterItem> ListFilter { get; set; } = new List<FilterItem>();
        public Produk DataProduk { get; set; } = new Produk();
        public searchParam SearchQuery { get; set; } = new searchParam();

        private string SearchText = "";
        private string ShortActive = "Name, A - Z";
        private bool ShortDirect = true;

        private System.Timers.Timer? _debounceTimer;
        [Parameter] public string? Keyword { get; set; }
        [Parameter] public int Quantity { get; set; } = 1;


        public SourchData dataSourch = new();
        public int searchCount { get; set; } = 0;
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public bool ShowOneLine { get; set; } = false;

        MyHeader header_page;
        MyFooter footer_page;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                var Phone = user.FindFirst(c => c.Type == ClaimTypes.OtherPhone)?.Value;
                var Password = user.FindFirst(c => c.Type == ClaimTypes.PrimarySid)?.Value;
                var Nama_display = user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
                var No_Id = user.FindFirst(c => c.Type == ClaimTypes.Sid)?.Value;

                var LoginData = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == No_Id);

                if (LoginData == null)
                {
                    var param = new ParameterApi
                    {
                        routeApi = "auth",
                        prosesApi = "login",
                        ParamUrl = new ParameterApi.ParameterUrl()
                    };

                    var loginStatus = await NewPdjService.LoginUserApi(param, new LoginViewModel { NoTlp = Phone, Password = Password });

                    if (loginStatus)
                    {
                        ScopedDataServices.ScopedData.DataLogin = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == No_Id);
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                        ScopedDataServices.ScopedData.DataLogin = null;
                    }
                }
                else
                {
                    ScopedDataServices.ScopedData.DataLogin = LoginData;
                }
            }
            else
            {
                ScopedDataServices.ScopedData.DataLogin = null;
            }
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

                if (queryParams.TryGetValue("brand", out var dataBrand))
                {
                    SearchQuery.Brand = Encoding.UTF8.GetString(Convert.FromBase64String(dataBrand));
                }

                SearchText = SearchQuery.Keyword;

                var CartTask = NewPdjService.RefreshDataMainKonten();
                
                var searchTask = pdjService.GetSearchApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "search",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        keyword = SearchQuery.Keyword,
                        kategoriid_list = SearchQuery.Kategori,
                        brandid_list = SearchQuery.Brand,
                        noid_list = SearchQuery.Produk,
                        strict_kategori = true,
                        pagesize = 12,
                        show_variant = true,
                        show_media = "true",
                        cursor_id = "",
                        cursor_nama = "",
                        cursor_harga = "",
                        orderBy = ShortActive == "Name, A - Z" || ShortActive == "Name, Z - A" ? "nama_display" : "harga",
                        orderDir = ShortDirect ? "asc" : "desc"
                    }
                });


                // Tunggu semua task selesai
                await Task.WhenAll(searchTask, CartTask);

                dataSourch.SearchList = searchTask.Result;



                if (!string.IsNullOrWhiteSpace(SearchQuery.Kategori))
                {
                    foreach (var idKat in SearchQuery.Kategori.Split(","))
                    {
                        var kate = dataSourch.ListDataKategori.FirstOrDefault(x => x.No_Id == idKat);

                        if (kate != null)
                        {
                            ListFilter.Add(new FilterItem { No_Id = kate.No_Id, Nama_Display = kate.Nama_Kategori, Checked = true });
                            kate.Checked_Filter = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(SearchQuery.Brand))
                {
                    foreach (var idBrand in SearchQuery.Brand.Split(","))
                    {
                        var kate = GlobalDataService.GlobalData.ListDataBrand.FirstOrDefault(x => x.No_Id == idBrand);

                        if (kate != null)
                        {
                            ListFilter.Add(new FilterItem { No_Id = kate.No_Id, Nama_Display = kate.Nama_Brand, Checked = true });
                            kate.Checked_Filter = true;
                        }
                    }
                }

                dataSourch.ListDataKategori.AddRange(GlobalDataService.GlobalData.ListDataKategori);
                dataSourch.ListDataBrand.AddRange(GlobalDataService.GlobalData.ListDataBrand);
                dataSourch.Finised = true;
                ScopedDataServices.ScopedData.Finised = true;
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("scrollToTop");
                await JSRuntime.InvokeVoidAsync("initslideProduct");

                loadContent = false;
                StateHasChanged();
            }

        }


        private void OnShortChanged(string value, bool orderDir)
        {
            ShortActive = value;
            ShortDirect = orderDir;

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


        private async void OnClearInput()
        {
            SearchText = "";

            dataSourch.SearchList = (new List<Brand>(), new List<Produk>(), 0, false);

            searchCount = 0;

            await SearchDataAsync(SearchText);
            StateHasChanged();
        }


        private async void OnChangeViewSm(bool value)
        {
            ShowOneLine = value;
            StateHasChanged();
        }

        public async Task selectProduk(string id_kode)
        {

            var produkTask = await pdjService.GetProdukApi(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 2, show_variant = true, max_media = 10, no_id = id_kode }
            });

            DataProduk = produkTask.FirstOrDefault(x => x.No_Id == id_kode);

            if (DataProduk == null)
            {
                await JSRuntime.InvokeVoidAsync("initSearchButton");
                StateHasChanged();
                return;
            }

            var dataProd = DataProduk?.Variants.FirstOrDefault(x => x.Stock >= DataProduk?.Min_Order);

            var Opsi = DataProduk?.Options.FirstOrDefault(x => x.Option_Id == dataProd.Option_Id);
            DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            var Size = DataProduk?.Sizes.FirstOrDefault(x => x.Size_Id == dataProd.Size_Id);
            DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            DataProduk.Size_Default = Size?.Size_Label ?? "";

            Quantity = DataProduk?.Min_Order ?? 0;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");
        }

        public async Task removeAllFilter()
        {

            foreach (var filter in ListFilter)
            {
                var Item = dataSourch.ListDataKategori.FirstOrDefault(x => x.No_Id == filter.No_Id && x.Nama_Kategori == filter.Nama_Display);
                if (Item != null)
                {
                    Item.Checked_Filter = false;
                }

                var Item1 = dataSourch.ListDataBrand.FirstOrDefault(x => x.No_Id == filter.No_Id && x.Nama_Brand == filter.Nama_Display);
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
                var Item = dataSourch.ListDataKategori.FirstOrDefault( x => x.No_Id == dataKategori.No_Id);
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
                var Item = dataSourch.ListDataBrand.FirstOrDefault(x => x.No_Id == dataBrand.No_Id);
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
                var ItemBrand = dataSourch.ListDataBrand.FirstOrDefault(x => x.No_Id == dataFilter.No_Id);
                var ItemKategori = dataSourch.ListDataKategori.FirstOrDefault( x => x.No_Id == dataFilter.No_Id);

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
            dataSourch.SearchList = await pdjService.GetSearchApi(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "search",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    keyword = query,
                    kategoriid_list = string.Join(",", dataSourch.ListDataKategori.Where(x => x.Checked_Filter == true).Select(x => x.No_Id)),
                    brandid_list = string.Join(",", dataSourch.ListDataBrand.Where(x => x.Checked_Filter == true).Select( x => x.No_Id) ),
                    strict_kategori = true,
                    pagesize = 12,
                    show_variant = true,
                    show_media = "true",
                    cursor_id = "",
                    cursor_nama = "",
                    cursor_harga =  "",
                    orderBy = ShortActive == "Name, A - Z" || ShortActive == "Name, Z - A" ? "nama_display" : "harga",
                    orderDir = ShortDirect ? "asc" : "desc"
                }
            });

            searchCount = dataSourch.SearchList.TotalCount == 0 ? -1 : dataSourch.SearchList.TotalCount;

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
                    keyword = SearchText,
                    kategoriid_list = string.Join(",", dataSourch.ListDataKategori.Where(x => x.Checked_Filter == true).Select(x => x.No_Id)),
                    brandid_list = string.Join(",", dataSourch.ListDataBrand.Where(x => x.Checked_Filter == true).Select(x => x.No_Id)),
                    strict_kategori = true,
                    pagesize = 12,
                    show_variant = true,
                    show_media = "true",
                    cursor_id = dataSourch.SearchList.SearchProduk.LastOrDefault()?.No_Id ?? "",
                    cursor_nama = ShortActive == "Name, A - Z" || ShortActive == "Name, Z - A" ? dataSourch.SearchList.SearchProduk.LastOrDefault()?.Nama_Display ?? "" : "",
                    cursor_harga = ShortActive == "Name, A - Z" || ShortActive == "Name, Z - A" ? "" : dataSourch.SearchList.SearchProduk.LastOrDefault()?.Harga.ToString() ?? "",
                    orderBy = ShortActive == "Name, A - Z" || ShortActive == "Name, Z - A" ? "nama_display" : "harga",
                    orderDir = ShortDirect ? "asc" : "desc"
                }
            });

            var data = dataSourch.SearchList.SearchProduk;
            data.AddRange(NewList.SearchProduk);

            dataSourch.SearchList = (NewList.SearchBrand, data, NewList.TotalCount, NewList.hasMore);

            searchCount = dataSourch.SearchList.TotalCount == 0 ? -1 : dataSourch.SearchList.TotalCount;

            loadProses = false;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initslideProduct");
        }
    }
}
