using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using WebShared.Models;
using Newtonsoft.Json.Linq;
using PacificWeb.Components.Layout;
using PacificWeb.Components.Sections;
using Radzen;
using Radzen.Blazor;
using WebShared.Services;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static WebShared.Models.Pesanan;
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

        private DotNetObjectReference<SearchPage>? _dotNetRef;

        public class FilterItem
        {
            public string Nama_Display { get; set; }
            public string No_Id { get; set; }
            public string Tipe { get; set; }
            public bool Checked { get; set; }
        }
        public class searchParam
        {
            public string Keyword { get; set; }
            public string Kategori { get; set; }
            public string Produk { get; set; }
            public string Navbar { get; set; }
            public string Brand { get; set; }
            public int? minHarga { get; set; }
            public int? maxHarga { get; set; }
            public bool sort { get; set; }
            public bool allowRange { get; set; }
            public string sortBy { get; set; }
        }

        public List<FilterItem> ListFilter { get; set; } = new List<FilterItem>();
        public List<Kategori> ListKagori { get; set; } = new List<Kategori>();
        public List<Brand> ListBrand { get; set; } = new List<Brand>();
        public Produk DataProduk { get; set; } = new Produk();
        public searchParam SearchQuery { get; set; } = new searchParam();

        private string SearchText = "";
        private string JudulPage = "Hasil Pencarian";
        private string ShortActive = "Nama, A - Z";
        private bool ShortDirect = true;

        private System.Timers.Timer? _debounceTimer;
        [Parameter] public string? Keyword { get; set; }
        [Parameter] public int Quantity { get; set; } = 1;


        public SourchData dataSourch = new();
        public int searchCount { get; set; } = 0;
        public int minHarga { get; set; } = 0;
        public int maxHarga { get; set; } = 0;
        public int Termahal { get; set; } = 0;
        public int Termurah { get; set; } = 0;
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public bool firstInject { get; set; } = true;
        public bool allowHarga { get; set; } = false;
        public bool ShowOneLine { get; set; } = false;

        MyHeader header_page;
        MyFooter footer_page;
        SelectProduk select_produk;

        protected override async Task OnInitializedAsync()
        {

            var dataDashboard = await NewPdjService.GetApiHomePage<DataHomePage>(new ParameterApi
            {
                routeApi = "bulk",
                prosesApi = "homepage",
                ParamUrl = new ParameterApi.ParameterUrl { id_company = "comp_1", produk_limit = 12 }
            });

            GlobalDataService.GlobalData.DataHomepage.UpdateFrom(dataDashboard);

            StateHasChanged();

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

            await NewPdjService.GetListGlobalDataAsync<Navbar>(new ParameterApi
            {
                routeApi = "navbar",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_id = "comp_1",
                    status = "aktif"
                }
            });
        }



        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("scrollToTop");

                var uri = new Uri(NavigationManager.Uri);
                var queryParams = QueryHelpers.ParseQuery(uri.Query);

                if (queryParams.TryGetValue("keyword", out var dataKeyword))
                {
                    SearchQuery.Keyword = Encoding.UTF8.GetString(Convert.FromBase64String(dataKeyword));
                }

                if (queryParams.TryGetValue("promo", out var dataProduk))
                {
                    SearchQuery.Navbar = Encoding.UTF8.GetString(Convert.FromBase64String(dataProduk));

                    if(SearchQuery.Navbar != null || SearchQuery.Navbar != "")
                    {
                        var navbar = GlobalDataService?.GlobalData?.ListDataNavbar?.FirstOrDefault(x => x.Nav_Type == SearchQuery.Navbar);

                        if (navbar?.Filter != null)
                        {
                            SearchQuery.Produk = string.Join(",", navbar.Filter);
                            JudulPage = navbar.Display_Text;
                            StateHasChanged();
                        }

                    }

                }

                if (queryParams.TryGetValue("kategori", out var dataKategori))
                {
                    SearchQuery.Kategori = Encoding.UTF8.GetString(Convert.FromBase64String(dataKategori));
                }

                if (queryParams.TryGetValue("brand", out var dataBrand))
                {
                    SearchQuery.Brand = Encoding.UTF8.GetString(Convert.FromBase64String(dataBrand));
                }

                try
                {
                    if (queryParams.TryGetValue("minharga", out var dataMin))
                    {
                        SearchQuery.minHarga = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(dataMin)));
                    }
                }
                catch
                {
                    SearchQuery.minHarga = null;
                }

                try
                {
                    if (queryParams.TryGetValue("maxharga", out var dataMax))
                    {
                        SearchQuery.maxHarga = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(dataMax)));
                    }
                }
                catch
                {
                    SearchQuery.maxHarga = null;
                }


                try
                {
                    if (queryParams.TryGetValue("sort", out var sort))
                    {
                        var datasort = Encoding.UTF8.GetString(Convert.FromBase64String(sort));
                        SearchQuery.sort = Convert.ToBoolean(datasort != null || datasort != "" ? datasort : "true");
                    } 
                    else
                    {
                        SearchQuery.sort = true;
                    }
                }
                catch
                {
                    SearchQuery.sort = true;
                }

                try
                { 
                    if (queryParams.TryGetValue("allowRange", out var allowRange))
                    {
                        SearchQuery.allowRange = Convert.ToBoolean(Encoding.UTF8.GetString(Convert.FromBase64String(allowRange)));
                    }
                }
                catch
                {
                    SearchQuery.allowRange = false;
                }


                if (queryParams.TryGetValue("sortBy", out var sortBy))
                {
                    SearchQuery.sortBy = Encoding.UTF8.GetString(Convert.FromBase64String(sortBy));
                }

                SearchText = SearchQuery.Keyword;

                var CartTask = NewPdjService.RefreshDataMainKonten();
                var react = NewPdjService.RefreshDataAuthKonten();


                // Tunggu semua task selesai
                await Task.WhenAll(CartTask, react);
                StateHasChanged();


                dataSourch.ListDataKategori.Clear();
                dataSourch.ListDataBrand.Clear();

                dataSourch.ListDataKategori.AddRange(GlobalDataService.GlobalData.ListDataKategori);
                dataSourch.ListDataBrand.AddRange(GlobalDataService.GlobalData.ListDataBrand);


                foreach (var item in dataSourch.ListDataKategori.Where(x => x.Checked_Filter == true).ToList())
                {
                    item.Checked_Filter = false;
                }

                foreach (var item in dataSourch.ListDataBrand.Where(x => x.Checked_Filter == true).ToList())
                {
                    item.Checked_Filter = false;
                }

                if (!string.IsNullOrWhiteSpace(SearchQuery.Kategori))
                {
                    foreach (var idKat in SearchQuery.Kategori.Split(","))
                    {
                        var kate = dataSourch.ListDataKategori.FirstOrDefault(x => x.No_Id == idKat);

                        if (kate != null)
                        {
                            ListFilter.Add(new FilterItem { No_Id = kate.No_Id, Nama_Display = kate.Nama_Kategori, Tipe = "kategori", Checked = true });
                            kate.Checked_Filter = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(SearchQuery.Brand))
                {
                    foreach (var idBrand in SearchQuery.Brand.Split(","))
                    {
                        var kate = dataSourch.ListDataBrand.FirstOrDefault(x => x.No_Id == idBrand);

                        if (kate != null)
                        {
                            ListFilter.Add(new FilterItem { No_Id = kate.No_Id, Nama_Display = kate.Nama_Brand, Tipe = "brand", Checked = true });
                            kate.Checked_Filter = true;
                        }
                    }
                }


                if (SearchQuery.allowRange)
                {
                    if (SearchQuery?.maxHarga > 0)
                    {
                        ListFilter.Add(new FilterItem { Nama_Display = $"{string.Format("Rp. {0:N0}", SearchQuery.minHarga)} - {string.Format("Rp. {0:N0}", SearchQuery.maxHarga)}", No_Id = "FilterHarga", Tipe = "harga", Checked = true });
                    }
                }

                ShortActive = SearchQuery?.sortBy != null ? SearchQuery?.sortBy : "Nama, A - Z";
                ShortDirect = SearchQuery?.sort != null ? SearchQuery.sort : true;
                allowHarga = SearchQuery.allowRange;

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
                        min_harga = allowHarga ? SearchQuery.minHarga.ToString() : null,
                        max_harga = allowHarga ? (SearchQuery.maxHarga <= 0 ? null : SearchQuery.maxHarga.ToString()) : null,
                        orderBy = ShortActive == "Nama, A - Z" || ShortActive == "Nama, Z - A" || ShortActive == "" || ShortActive == null ? "nama_display" : "harga",
                        orderDir = SearchQuery.sort ? "asc" : "desc"
                    }
                });


                await Task.WhenAll(searchTask);
                dataSourch.SearchList = searchTask.Result;

                Termahal = (int)(Math.Ceiling(dataSourch.SearchList.priceMax / 100000.0) * 100000);

                minHarga = allowHarga ? (SearchQuery.minHarga ?? 0) : 0;
                maxHarga = allowHarga ? (SearchQuery.maxHarga ?? (int)(Math.Ceiling(dataSourch.SearchList.priceMax / 100000.0) * 100000)) : (int)(Math.Ceiling(dataSourch.SearchList.priceMax / 100000.0) * 100000);
                searchCount = dataSourch.SearchList.TotalCount == 0 ? -1 : dataSourch.SearchList.TotalCount;


                dataSourch.Finised = true;
                ScopedDataServices.ScopedData.Finised = true;
                loadContent = false;
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("initslideProduct");

                StateHasChanged();

                _dotNetRef = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("initRangeHarga", _dotNetRef);
                StateHasChanged();
            }

        }

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(SearchText))
            {
                var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
                NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", false);
            }
        }

        private void SearchCari()
        {
            var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
            NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", false);
        }

        [JSInvokable]
        public async Task OnPriceChanged(int minPrice, int maxPrice)
        {
            if(firstInject)
            {
                firstInject = false;
            } 
            else
            {
                var data = ListFilter.FirstOrDefault(x => x.No_Id == "FilterHarga");

                if (data == null)
                {
                    ListFilter.Add(new FilterItem { Nama_Display = $"{string.Format("Rp. {0:N0}", minPrice)} - {string.Format("Rp. {0:N0}", maxPrice)}", No_Id = "FilterHarga", Checked = true});
                }
                else
                {
                    data.Nama_Display = $"{string.Format("Rp. {0:N0}", minPrice)} - {string.Format("Rp. {0:N0}", maxPrice)}";
                }

                maxHarga = maxPrice;
                minHarga = minPrice;
                allowHarga = true;

                loadProses = true;
                StateHasChanged();

                await SearchDataAsync(SearchText);

                loadProses = false;
                StateHasChanged();
            }

            StateHasChanged();
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

            dataSourch.SearchList = (new List<Brand>(), new List<Produk>(), 0, false,0);

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

            var dataProd = ScopedDataServices.ScopedData.DataProduk?.Variants.FirstOrDefault(x => x.Stock >= ScopedDataServices.ScopedData.DataProduk?.Min_Order);

            var Opsi = ScopedDataServices.ScopedData.DataProduk?.Options.FirstOrDefault(x => x.Option_Id == dataProd.Option_Id);
            ScopedDataServices.ScopedData.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            var Size = ScopedDataServices.ScopedData.DataProduk?.Sizes.FirstOrDefault(x => x.Size_Id == dataProd.Size_Id);
            ScopedDataServices.ScopedData.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Size_Default = Size?.Size_Label ?? "";
            select_produk.Quantity = ScopedDataServices.ScopedData.DataProduk?.Min_Order ?? 1;

            Quantity = ScopedDataServices.ScopedData.DataProduk?.Min_Order ?? 0;
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

                if (filter.No_Id == "FilterHarga")
                {
                    await JSRuntime.InvokeVoidAsync("initResetFilter", Termahal);
                    allowHarga = false;
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

                if (Data == null) ListFilter.Add(new FilterItem { No_Id = Item.No_Id, Tipe = "kategori", Nama_Display = Item.Nama_Kategori, Checked = true });
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

                if (Data == null) ListFilter.Add(new FilterItem { No_Id = Item.No_Id, Tipe = "brand", Nama_Display = Item.Nama_Brand, Checked = true });
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
                }

                if (ItemKategori != null)
                {
                    ItemKategori.Checked_Filter = false;
                }

                if (dataFilter.No_Id == "FilterHarga")
                {
                    await JSRuntime.InvokeVoidAsync("initResetFilter", Termahal);
                    allowHarga = false;
                }

                    ListFilter.Remove(dataFilter);
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
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: false);
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
            SearchCombo();

            dataSourch.SearchList = await pdjService.GetSearchApi(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "search",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    keyword = query,
                    kategoriid_list = string.Join(",", dataSourch.ListDataKategori.Where(x => x.Checked_Filter == true).Select(x => x.No_Id)),
                    brandid_list = string.Join(",", dataSourch.ListDataBrand.Where(x => x.Checked_Filter == true).Select(x => x.No_Id)),
                    noid_list = SearchQuery.Produk != null && SearchQuery.Produk != "" ? SearchQuery.Produk : null,
                    strict_kategori = true,
                    pagesize = 12,
                    show_variant = true,
                    show_media = "true",
                    cursor_id = "",
                    cursor_nama = "",
                    cursor_harga = "",
                    min_harga = allowHarga ? minHarga.ToString() : null,
                    max_harga = allowHarga ? maxHarga.ToString() : null,
                    orderBy = ShortActive == "Nama, A - Z" || ShortActive == "Nama, Z - A" ? "nama_display" : "harga",
                    orderDir = ShortDirect ? "asc" : "desc"
                }
            });



            searchCount = dataSourch.SearchList.TotalCount == 0 ? -1 : dataSourch.SearchList.TotalCount;
            //Termahal = dataSourch.SearchList.priceMax;


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
                    noid_list = SearchQuery.Produk != null && SearchQuery.Produk != "" ? SearchQuery.Produk : null,
                    strict_kategori = true,
                    pagesize = 12,
                    show_variant = true,
                    show_media = "true",
                    min_harga = allowHarga ? minHarga.ToString() : null,
                    max_harga = allowHarga ? maxHarga.ToString() : null,
                    cursor_id = dataSourch.SearchList.SearchProduk.LastOrDefault()?.No_Id ?? "",
                    cursor_nama = ShortActive == "Nama, A - Z" || ShortActive == "Nama, Z - A" ? dataSourch.SearchList.SearchProduk.LastOrDefault()?.Nama_Display ?? "" : "",
                    cursor_harga = ShortActive == "Nama, A - Z" || ShortActive == "Nama, Z - A" ? "" : dataSourch.SearchList.SearchProduk.LastOrDefault()?.Harga.ToString() ?? "",
                    orderBy = ShortActive == "Nama, A - Z" || ShortActive == "Nama, Z - A" ? "nama_display" : "harga",
                    orderDir = ShortDirect ? "asc" : "desc"
                }
            });

            var data = dataSourch.SearchList.SearchProduk;
            data.AddRange(NewList.SearchProduk);

            //maxHarga = dataSourch.SearchList.priceMax;
            dataSourch.SearchList = (NewList.SearchBrand, data, NewList.TotalCount, NewList.hasMore, NewList.priceMax);


            searchCount = dataSourch.SearchList.TotalCount == 0 ? -1 : dataSourch.SearchList.TotalCount;

            loadProses = false;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initslideProduct");
        }

        public void SearchCombo()
        {
            var bra = "";
            var kate = "";
            var keyword = "";
            var sort = "";
            var sortBy = "";
            var minHrg = "";
            var maxHrg = "";
            var allowRange = "";
            var promo = "";

            if (ListFilter != null)
            {
                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", ListFilter.Where(x => x.Tipe == "brand").ToList().Select(x => x.No_Id))));
                kate = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", ListFilter.Where(x => x.Tipe == "kategori").ToList().Select(x => x.No_Id))));
                keyword = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
                sort = Convert.ToBase64String(Encoding.UTF8.GetBytes(ShortDirect.ToString()));
                sortBy = Convert.ToBase64String(Encoding.UTF8.GetBytes(ShortActive));
                minHrg = Convert.ToBase64String(Encoding.UTF8.GetBytes(minHarga.ToString()));
                maxHrg = Convert.ToBase64String(Encoding.UTF8.GetBytes(maxHarga.ToString()));
                allowRange = Convert.ToBase64String(Encoding.UTF8.GetBytes(allowHarga.ToString()));
                promo = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.IsNullOrEmpty(SearchQuery.Navbar) ? "" : SearchQuery?.Navbar?.ToString()));
            }

            NavigationManager.NavigateTo($"/search-page?promo={Uri.EscapeDataString(promo)}&keyword={Uri.EscapeDataString(keyword)}&kategori={Uri.EscapeDataString(kate)}&brand={Uri.EscapeDataString(bra)}&minHarga={Uri.EscapeDataString(minHrg)}&maxHarga={Uri.EscapeDataString(maxHrg)}&sort={Uri.EscapeDataString(sort)}&sortBy={Uri.EscapeDataString(sortBy)}&allowRange={Uri.EscapeDataString(allowRange)}", replace: true);
        }
    }
}
