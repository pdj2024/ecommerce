using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using WebShared.Models;
using Radzen;
using WebShared.Services;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using static WebShared.Models.Pesanan;


namespace PacificWeb.Components.Layout
{
    public partial class MyHeader
    {
        private DataUser loginModel = new();
        private string ErrorMessage;

        [Parameter] public bool ShowModal { get; set; } = true;
        [Parameter] public EventCallback<bool> ShowModalChanged { get; set; }
        [Parameter] public EventCallback<string> OnLoginSuccess { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        private string SearchText = "";
        private string email = "";
        private string uid = "";
        private string token = "";
        private string pass = "";
        private int infoSubmit = 0;
        private bool LoadVisible = true;

        private System.Timers.Timer? _debounceTimer;

        public List<Cart> ListDataCartCheck { get; set; } = new List<Cart>();
        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore, int maxPrice) SearchList { get; set; } = (new(), new(), 0, false,0);

        [SupplyParameterFromForm]
        public LoginViewModel Model { get; set; } = new();

        [SupplyParameterFromForm]
        public LoginViewModel ModelRegist { get; set; } = new();


        public bool loadLogin { get; set; } = false;
        public bool checkAll { get; set; } = true;
        public string Numberinfo { get; set; } = "";
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public bool loadProsesNetto { get; set; } = false;
        public decimal? nettoKeranjang { get; set; } = 0;
        public int searchCount { get; set; } = 0;
        public int dataQty { get; set; } = 0;
        public bool InfoInitFlag { get; set; } = false;
        public bool disabeldAdd { get; set; } = false;
        public ClaimsPrincipal user { get; set; } = new();

        public int dataChange { get; set; } = 0;


        public void NavigateToHome(string route, bool force = false)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: force);
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.User;
        }

        private async Task Logout()
        {
            NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
        }

        async Task FocusSearch()
        {
            await Task.Delay(200);
            await JSRuntime.InvokeVoidAsync("focusInput", "searchFieldSelect");
        }

        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var companyTask = NewPdjService.GetListGlobalDataAsync<Company>(new ParameterApi
                {
                    routeApi = "company",
                    prosesApi = "get-profile",
                    ParamUrl = new ParameterApi.ParameterUrl { no_id = "comp_1" }
                });

                if(GlobalDataService.GlobalData.ListDataNavbar != null && GlobalDataService.GlobalData.ListDataNavbar.Count() >= 1)
                {
                    GlobalDataService.GlobalData.ListDataNavbar.AddRange(GlobalDataService.GlobalData.DataHomepage.Navbar);
                }

                await Task.WhenAll(companyTask);

                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initHeader");
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initMobileButton");
                await JSRuntime.InvokeVoidAsync("initHeaderMega");

                if (user.Identity?.IsAuthenticated ?? false)
                {

                    var CartTask = NewPdjService.GetApi<Cart>(new ParameterApi
                    {
                        routeApi = "cart",
                        prosesApi = "getAll",
                        token = ScopedDataServices.ScopedData.DataLogin.token,
                        refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                        ParamUrl = new ParameterApi.ParameterUrl
                        {
                            id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                            show_media = "true",
                            orderBy = "Updated_At",
                            orderDir = "desc",
                            status = "aktif"
                        }
                    });

                    var SummaryPesananTask = NewPdjService.GetApi<Pesanan.SummaryResponse>(new ParameterApi
                    {
                        routeApi = "order",
                        prosesApi = "getSummary",
                        token = ScopedDataServices.ScopedData.DataLogin.token,
                        refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                        ParamUrl = new ParameterApi.ParameterUrl
                        {
                            id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                        }
                    });

                    await Task.WhenAll(SummaryPesananTask, CartTask);

                    ScopedDataServices.ScopedData.ListDataSummaryPesanan = SummaryPesananTask.Result?.Data ?? new();
                    ScopedDataServices.ScopedData.ListDataCart = CartTask.Result?.Data ?? new();

                    await SelectItemCart(null);
                } 
                else if (!NewPdjService.featurAllow.CartLogin)
                {
                    var dataCart = await NewPdjService.GetLocalAsync<List<Cart>>("CartGuest");

                    ScopedDataServices.ScopedData.ListDataCartLocal = dataCart ?? new();

                    if (ScopedDataServices?.ScopedData?.ListDataCartLocal?.FirstOrDefault() == null) return;

                    var produk = await NewPdjService.GetApi<Produk>(new ParameterApi
                    {
                        routeApi = "produk",
                        prosesApi = "getAll",
                        ParamUrl = new ParameterApi.ParameterUrl
                        {
                            show_variant = true,
                            noid_list = string.Join(",", ScopedDataServices.ScopedData.ListDataCartLocal.Select(x => x.Id_Produk))
                        }
                    });

                    ScopedDataServices.ScopedData.ListDataCart.Clear();

                    foreach (var datacart in ScopedDataServices?.ScopedData?.ListDataCartLocal)
                    {
                        Cart itemCart = new Cart();
                        itemCart = datacart;

                        var data = produk?.Data?.FirstOrDefault(x => x.No_Id == itemCart.Id_Produk);
                        var variant = data?.Variants?.FirstOrDefault(x => x.No_Id == itemCart.Id_Variant);

                        if (data == null || variant == null) continue;

                        itemCart.Status_Produk = data.Status;
                        itemCart.Status_Variant = variant.Status;
                        itemCart.Option_Id = variant.Option_Id;
                        itemCart.Option_Label = variant.Option_Label;
                        itemCart.Size_Id = variant.Size_Id;
                        itemCart.Size_Label = variant.Size_Label;
                        itemCart.Harga = variant.Harga;
                        itemCart.Nama_Display = data.Nama_Display;
                        itemCart.Stock = variant.Stock;
                        itemCart.Diskon1 = data.Diskon1;
                        itemCart.Diskon2 = data.Diskon2;
                        itemCart.Diskon3 = data.Diskon3;
                        itemCart.Diskon4 = data.Diskon4;
                        itemCart.Id_Media = data.MediaUrls.FirstOrDefault()?.Id_Media;
                        itemCart.SelectCart = true;

                        ScopedDataServices.ScopedData.ListDataCart.Add(itemCart);
                    }

                    await SelectItemCart(null);
                }

                await JSRuntime.InvokeVoidAsync("initButtonFlagChange");
                await JSRuntime.InvokeVoidAsync("initButtonFlag");

                StateHasChanged();
            }
        }


        public async Task AddCartLocal(string data_id, Cart datacart, bool update = false)
        {
            if(update)
            {
                var datacartlist = ScopedDataServices.ScopedData.ListDataCart.FirstOrDefault(x => x.Id_Cart == datacart.Id_Cart);

                datacartlist.Qty += datacart.Qty;
            }
            else
            {
                Cart itemCart = new Cart();
                itemCart = datacart;

                var produk = await NewPdjService.GetApi<Produk>(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        show_variant = true,
                        no_id = data_id
                    }
                });

                var data = produk?.Data?.FirstOrDefault(x => x.No_Id == itemCart.Id_Produk);
                var variant = data?.Variants?.FirstOrDefault(x => x.No_Id == itemCart.Id_Variant);

                if (data == null || variant == null) return;

                itemCart.Status_Produk = data.Status;
                itemCart.Status_Variant = variant.Status;
                itemCart.Option_Id = variant.Option_Id;
                itemCart.Option_Label = variant.Option_Label;
                itemCart.Size_Id = variant.Size_Id;
                itemCart.Size_Label = variant.Size_Label;
                itemCart.Harga = variant.Harga;
                itemCart.Nama_Display = data.Nama_Display;
                itemCart.Stock = variant.Stock;
                itemCart.Diskon1 = data.Diskon1;
                itemCart.Diskon2 = data.Diskon2;
                itemCart.Diskon3 = data.Diskon3;
                itemCart.Diskon4 = data.Diskon4;
                itemCart.Id_Media = data.MediaUrls.FirstOrDefault()?.Id_Media;
                itemCart.SelectCart = true;

                ScopedDataServices.ScopedData.ListDataCart.Add(itemCart);
            }

            StateHasChanged();
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
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        OnClearInput();
                        return;
                    }
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
                    pagesize = 8,
                    keyword = query,
                    orderDir = "asc",
                    orderBy = "nama_display"
                }
            });

            searchCount = SearchList.TotalCount == 0 ? -1 : SearchList.TotalCount;

            StateHasChanged();
        }

        private void checkAllItem(ChangeEventArgs e)
        {
            checkAll = Convert.ToBoolean(e.Value);  
            if (checkAll)
            {
                nettoKeranjang = 0;
                if (ScopedDataServices?.ScopedData?.ListDataCart == null) return; 
                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (!dataCart.SelectCart) 
                    {
                        dataCart.SelectCart = true;
                    }

                    nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                }
            } else
            {
                nettoKeranjang = 0;
                if (ScopedDataServices?.ScopedData?.ListDataCart == null) return;
                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (dataCart.SelectCart)
                    {
                        dataCart.SelectCart = false;
                    }
                }
            }
        }

        private void OnClearInput()
        {
            SearchText = "";

            SearchList = (new List<Brand>(), new List<Produk>(), 0, false, 0);

            searchCount = 0;

            StateHasChanged();
        }

        public async Task RefreshCart(string data_id = null)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "getAll",
                token = ScopedDataServices?.ScopedData?.DataLogin.token,
                refreshToken = ScopedDataServices?.ScopedData?.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderBy = "Updated_At",
                    orderDir = "desc"
                }
            };
            ListDataCartCheck.Clear();
            ListDataCartCheck.AddRange(ScopedDataServices?.ScopedData?.ListDataCart?.FindAll(x => x.SelectCart == true) ?? new());
            var DataCartList = await NewPdjService.GetApi<Cart>(paramApi);

            if(data_id == null)
            {
                ScopedDataServices.ScopedData.ListDataCart = DataCartList?.Data;
            } 
            else
            {
                var DataShow = ScopedDataServices.ScopedData.ListDataCart.FirstOrDefault(x => x.Id_Cart == data_id);
                DataShow.Qty = DataCartList.Data.FirstOrDefault()?.Qty ?? 0;
            }

            if (ListDataCartCheck.Count() >= 1 && ScopedDataServices?.ScopedData?.ListDataCart != null)
            {
                foreach (var item in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (ListDataCartCheck.Any(x => x.Id_Cart == item.Id_Cart))
                    {
                        item.SelectCart = true;
                    }
                }
            }

            await SelectItemCart(null);
            StateHasChanged();
        }

        public async Task SelectItemCart(Cart cart)
        {
            loadProsesNetto = true;
            StateHasChanged();

            if (cart != null)
            {
                var data = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
                data.SelectCart = !data.SelectCart;

                nettoKeranjang = 0;

                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                }

            }
            else
            {
                checkAllItem(new ChangeEventArgs { Value = checkAll });
            }

            loadProsesNetto = false;
            StateHasChanged();
        }

        public async Task deleteItemCart(Cart cart)
        {

            if (!NewPdjService.featurAllow.CartLogin && (user.Identity?.IsAuthenticated != null && user.Identity?.IsAuthenticated == true ? false : true))
            {
                var dataItem = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
                var dataItemLocal = ScopedDataServices?.ScopedData?.ListDataCartLocal.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);

                ScopedDataServices?.ScopedData?.ListDataCart.Remove(dataItem);
                ScopedDataServices?.ScopedData?.ListDataCartLocal.Remove(dataItemLocal);

                await NewPdjService.SetLocalAsync("CartGuest", ScopedDataServices.ScopedData.ListDataCart);

                if (ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault() != null) await SelectItemCart(null);
                StateHasChanged();

                return;
            }


            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "delete",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl 
                { 
                    no_id = cart.Id_Cart
                }
            };
            loadProses = true;
            StateHasChanged();
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("HAPUS", cart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                await RefreshCart();
            }

            loadProses = false;
            StateHasChanged();
        }

        private async Task Increase(string id_cart)
        {
            if (!NewPdjService.featurAllow.CartLogin && (user.Identity?.IsAuthenticated != null && user.Identity?.IsAuthenticated == true ? false : true))
            {
                var dataItem = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
                var dataItemLocal = ScopedDataServices?.ScopedData?.ListDataCartLocal.FirstOrDefault(x => x.Id_Cart == id_cart);

                dataItem.Qty++;
                //dataItemLocal.Qty++;

                await NewPdjService.SetLocalAsync("CartGuest", ScopedDataServices.ScopedData.ListDataCart);

                await SelectItemCart(null);
                StateHasChanged();

                return;
            }

            var data = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            

            if (NewPdjService.featurAllow.StockActive)
            {

                if (data.Qty + 1 > data?.Stock)
                {
                    data.Show_Max_Order = true;
                }
                else
                {
                    dataQty++;
                    data.Qty++;

                    if (data.Qty < data?.Min_Order)
                    {
                        data.Show_Min_Order = true;
                    }
                    else
                    {
                        data.Show_Min_Order = false;
                    }

                    data.Show_Max_Order = false;
                }


                if (data.Qty < data?.Min_Order || data.Qty > data?.Stock)
                {
                    disabeldAdd = true;
                }
                else
                {
                    disabeldAdd = false;
                }

            }
            else
            {
                dataQty++;
                data.Qty++;
            }


            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }

            data.Show_Min_Order = false;
            data.Show_Max_Order = false;

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl()
            };
            
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            _debounceTimer = new System.Timers.Timer(500); // 500ms delay
            _debounceTimer.Elapsed += async (s, e) =>
            {
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();

                await InvokeAsync(async () =>
                {

                    Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = dataQty, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id };
                    (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("TAMBAH", itemcart, paramApi);


                    if (statusRespon && statusCode == 200)
                    {
                        await RefreshCart(id_cart);
                    }


                    nettoKeranjang = 0;
                    foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                    {
                        if (!dataCart.SelectCart) continue;
                        nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                    }

                    dataQty = 0;
                });
            };

            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();


            loadProsesNetto = false;
            StateHasChanged();
        }

        private async Task Decrease(string id_cart)
        {

            if (!NewPdjService.featurAllow.CartLogin && (user.Identity?.IsAuthenticated != null && user.Identity?.IsAuthenticated == true ? false : true))
            {
                var dataItem = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
                var dataItemLocal = ScopedDataServices?.ScopedData?.ListDataCartLocal.FirstOrDefault(x => x.Id_Cart == id_cart);

                if (dataItem.Qty > 1)
                {
                    dataItem.Qty--;
                    //dataItemLocal.Qty--;

                    await NewPdjService.SetLocalAsync("CartGuest", ScopedDataServices.ScopedData.ListDataCart);
                    await SelectItemCart(null);
                }

                StateHasChanged();

                return;
            }

            var data = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);

            if (data.Qty <= data.Min_Order)
            {
                data.Show_Min_Order = true;
                data.Show_Max_Order = false;
                return;
            }
            
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }

            data.Show_Min_Order = false;
            data.Show_Max_Order = false;

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl()
            };


            if (data.Qty > 1)
            {
                dataQty--;
                data.Qty--;


                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();

                _debounceTimer = new System.Timers.Timer(500); // 500ms delay
                _debounceTimer.Elapsed += async (s, e) =>
                {
                    _debounceTimer?.Stop();
                    _debounceTimer?.Dispose();

                    await InvokeAsync(async () =>
                    {

                        Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = dataQty, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id  };
                        (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("TAMBAH", itemcart, paramApi);


                        if (statusRespon && statusCode == 200)
                        {
                            await RefreshCart(id_cart);
                        }


                        nettoKeranjang = 0;
                        foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                        {
                            if (!dataCart.SelectCart) continue;
                            nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                        }

                        dataQty = 0;
                    });
                };

                _debounceTimer.AutoReset = false;
                _debounceTimer.Start();
            }

            loadProsesNetto = false;
            StateHasChanged();
        }
        
        public async Task CheckoutCart() {

            string text = "";

            if (!NewPdjService.featurAllow.CartLogin)
            {
                await NewPdjService.SetLocalAsync("CartGuest", ScopedDataServices.ScopedData.ListDataCart);
            }

            foreach (var item in ScopedDataServices?.ScopedData?.ListDataCart)
            {
                if (item.SelectCart) text += $"{item.Id_Cart}%section%";
            }
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/cart/{encoded}", forceLoad: true);

        }

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(SearchText))
            {
                var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
                NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", true);
            }
        }
        
        private void SearchCari()
        {
            var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
            NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", true);
        }

        private void seeResult()
        {
            var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
            NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", true);
        }

        private void searchContent(Navbar Navbar)
        {
            string prod = "";
            string kate = "";

            if (Navbar.Nav_Type.Split("_")[0] == "special")
            {
                prod = Navbar.Nav_Type;
            } 
            else
            {
                kate = string.Join(",", Navbar.Kategori.Select(x => x.No_Id));
            }

            prod = Convert.ToBase64String(Encoding.UTF8.GetBytes(prod));
            kate = Convert.ToBase64String(Encoding.UTF8.GetBytes(kate));
            var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));

            NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}&promo={Uri.EscapeDataString(prod)}&kategori={Uri.EscapeDataString(kate)}", true);
        }

        public async void changeLogin(string value, bool direct = false)
        {
            loadLogin = true;
            StateHasChanged();

            if (InfoInitFlag == false)
            {
                InfoInitFlag = true;
            }

            if (value == "change-pw")
            {
                if (ModelRegist.NoTlp == "" || ModelRegist.NoTlp == null || ModelRegist.Password == "" || ModelRegist.Password == null)
                {
                    infoSubmit = 1;
                    StateHasChanged();
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }

                Numberinfo = "";
                Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlagChange");

                if (Numberinfo == "FALSE")
                {
                    infoSubmit = 2;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }

                var dataCek = await NewPdjService.CheckUserRegApi(new ParameterApi
                {
                    routeApi = "auth",
                    prosesApi = "check",
                    ParamUrl = new ParameterApi.ParameterUrl()
                }, new LoginViewModel { NoTlp = Numberinfo, Password = ModelRegist.Password });

                if (dataCek)
                {
                    infoSubmit = 3;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }


                var otp = await NewPdjService.SendOtp(new ParameterApi
                {
                    routeApi = "auth",
                    prosesApi = "request-otp",
                    ParamUrl = new ParameterApi.ParameterUrl()
                }, new { Telepon = Numberinfo, Check_User = false });

                if (otp)
                {
                    await JSRuntime.InvokeVoidAsync("initLoginEffect", "otp-view", direct);
                    dataChange = 2;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }
                else
                {
                    infoSubmit = 4;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }
            }

            if (value == "otp-view")
            {

                if (ModelRegist.NoTlp == "" || ModelRegist.NoTlp == null || ModelRegist.Password == "" || ModelRegist.Password == null)
                {
                    infoSubmit = 1;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }

                Numberinfo = "";
                Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlag");
                if (Numberinfo == "FALSE") {
                    infoSubmit = 2;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                } 
                else
                {

                    var dataCek = await NewPdjService.CheckUserRegApi(new ParameterApi
                    {
                        routeApi = "auth",
                        prosesApi = "check",
                        ParamUrl = new ParameterApi.ParameterUrl()
                    }, new LoginViewModel { NoTlp = Numberinfo, Password = ModelRegist.Password });

                    if (!dataCek)
                    {
                        infoSubmit = 3;
                        loadLogin = false;
                        StateHasChanged();
                        return;
                    }

                    var otp = await NewPdjService.SendOtp(new ParameterApi
                    {
                        routeApi = "auth",
                        prosesApi = "request-otp",
                        ParamUrl = new ParameterApi.ParameterUrl()
                    }, new { Telepon = Numberinfo, Check_User = false });

                    if (otp)
                    {
                        await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);
                        loadLogin = false;
                        StateHasChanged();
                        return;
                    }
                    else
                    {
                        infoSubmit = 4;
                        StateHasChanged();
                        loadLogin = false;
                        StateHasChanged();
                        return;
                    }
                }


            }
            
            else
            {
                infoSubmit = 0;
                Model.NoTlp = "";
                Model.Password = "";
                ModelRegist.NoTlp = "";
                ModelRegist.Password = "";
                noRegist = null;
                noLogin = null;

                loadLogin = false;
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);

            }

            loadLogin = false;
            StateHasChanged();
        }

        private async Task LoginStart()
        {
            infoSubmit = 0;
            StateHasChanged();

            if (Model.NoTlp == "" || Model.NoTlp == null || Model.Password == "" || Model.Password == null)
            {
                infoSubmit = 2;
                StateHasChanged();
                return;
            }

            Numberinfo = "";
            Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlagLogin");
            if (Numberinfo == "FALSE") return;


            var param = new ParameterApi
            {
                routeApi = "auth",
                prosesApi = "login",
                ParamUrl = new ParameterApi.ParameterUrl { }
            };

            var modelLog = new LoginViewModel { NoTlp = Numberinfo, Password = Model.Password };

            var loginStatus = await NewPdjService.CheckLoginApi(param, modelLog);

            if (loginStatus)
            {
                if(!NewPdjService.featurAllow.CartLogin && ScopedDataServices.ScopedData.ListDataCart?.Count() >= 1)
                {
                    ParameterApi paramApi = new ParameterApi
                    {
                        routeApi = "cart",
                        token = ScopedDataServices.ScopedData.DataLogin?.token,
                        refreshToken = ScopedDataServices.ScopedData.DataLogin?.refreshToken,
                        prosesApi = "create",
                        ParamUrl = new ParameterApi.ParameterUrl()
                    };

                    foreach (var addCart in ScopedDataServices.ScopedData.ListDataCart)
                    {
                        Cart itemCart = new Cart { Id_Variant = addCart.Id_Variant, Qty = addCart.Qty, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id };
                        await NewPdjService.CudCartApi("TAMBAH", itemCart, paramApi);
                    }

                    await NewPdjService.ClearLocalAsync();

                }

                var dataDecode = await pdjService.DecodeEncode(true, new List<string> { ScopedDataServices?.ScopedData?.DataLogin?.No_Id });
                NavigationManager.NavigateTo($"/auth-user-session/session/{dataDecode[0]}", forceLoad: true);
            } 
            else
            {
                infoSubmit = 1;
                StateHasChanged();
                return;
            }
        }

        public async void NavigateToDetailProduk(string id)
        {
            List<string> stringData = await pdjService.DecodeEncode(true, new List<string> { id });
            NavigationManager.NavigateTo($"/detail-produk/{stringData.FirstOrDefault()}", forceLoad: true);
        }

        public int? noRegist { get; set; } = null;
        public int? noLogin { get; set; } = null;

        public async Task NumberOnlyInput(ChangeEventArgs e, string info)
        {
            var value = e.Value?.ToString() ?? "";

            bool hasPlus = value.StartsWith("+");

            value = Regex.Replace(value, @"[^0-9]", "");

            if (hasPlus)
            {
                value = "+" + value;
            }

            if (info == "phone-login")
            {
                Model.NoTlp = value;
            } 
            else if (info == "phone")
            {
                ModelRegist.NoTlp = value;
            }
            else if (info == "phone-change")
            {
                ModelRegist.NoTlp = value;
            }

            await JSRuntime.InvokeVoidAsync("setInputValue", info, value);
        }

        public void searchCombo(Navbar Navbar = null, Brand brand = null, Kategori kategori = null)
        {
            var bra = "";

            var kate = "";

            if (Navbar == null)
            {
                if (brand != null)
                {
                    bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));
                    NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand={Uri.EscapeDataString(bra)}", true);
                    return;
                }
                else if (kategori != null)
                {
                    kate = Convert.ToBase64String(Encoding.UTF8.GetBytes((kategori.No_Id == null || kategori.No_Id == "" ? kategori.Id : kategori.No_Id)));
                    NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand=", true);
                    return;
                }
            }

            kate = string.Join(",", Navbar.Filter.Select(x => x));


            if (brand != null)
            {

                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));

            }
            else if (kategori != null)
            {
                kate = kate + $",{(kategori.No_Id == null || kategori.No_Id == "" ? kategori.Id : kategori.No_Id)}";
            }
            
            kate = Convert.ToBase64String(Encoding.UTF8.GetBytes(kate));

            NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand={Uri.EscapeDataString(bra)}", true);
        }

        private async Task ShowNavbar(string classItem, bool status)
        {
            await JSRuntime.InvokeVoidAsync("showNavbar", classItem, status);
        }

    }
}
