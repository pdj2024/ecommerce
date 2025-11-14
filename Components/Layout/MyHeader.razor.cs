using BlazorBootstrap;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json.Linq;
using PacificWeb.Components.Pages.Account;
using Radzen;
using Services;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using static Models.Pesanan;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PacificWeb.Components.Layout
{
    public partial class MyHeader
    {
        private DataUser loginModel = new();
        private string ErrorMessage;

        [Parameter] public bool ShowModal { get; set; }
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
        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new(), new(), 0, false);

        [SupplyParameterFromForm]
        public LoginViewModel Model { get; set; } = new();

        [SupplyParameterFromForm]
        public LoginViewModel ModelRegist { get; set; } = new();


        public bool checkAll { get; set; } = true;
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public bool loadProsesNetto { get; set; } = false;
        public decimal? nettoKeranjang { get; set; } = 0;
        public int searchCount { get; set; } = 0;
        public int dataQty { get; set; } = 0;
        public bool InfoInitFlag { get; set; } = false;

        public int dataChange { get; set; } = 0;


        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: true);
        }

        protected override async Task OnInitializedAsync()
        {

        }

        private async Task Logout()
        {
            NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                int count = 0;
                while (ScopedDataServices.ScopedData.Finised == false)
                {
                    if (count >= 6)
                    {
                        await Task.Delay(1000);

                        if (count >= 10)
                        {
                            await Task.Delay(1000);

                            Console.WriteLine("Gagal memuat data header & footer");

                            if (count >= 15)
                            {
                                ScopedDataServices.ScopedData.Finised = true;
                            }
                        }
                    } 
                    else
                    {
                        await Task.Delay(500);
                    }

                    count++;
                }




                loadContent = false;
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initMobileButton");
                await JSRuntime.InvokeVoidAsync("initHeaderMega");
                await JSRuntime.InvokeVoidAsync("colLoadOut");

                StateHasChanged();

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
                    keyword = query
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

            SearchList = (new List<Brand>(), new List<Produk>(), 0, false);

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
            ListDataCartCheck.AddRange(ScopedDataServices?.ScopedData?.ListDataCart?.FindAll(x => x.SelectCart == true));
            var DataCartList = await NewPdjService.GetApi<Cart>(paramApi);

            if(data_id == null)
            {
                ScopedDataServices.ScopedData.ListDataCart = DataCartList.Data;
            } 
            else
            {
                var DataShow = ScopedDataServices.ScopedData.ListDataCart.FirstOrDefault(x => x.Id_Cart == data_id);
                DataShow.Qty = DataCartList.Data.FirstOrDefault(x => x.Id_Cart == data_id).Qty;
            }

            if (ListDataCartCheck.Count() >= 1)
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
            var data = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            
            if (data.Qty >= data.Stock)
            {
                data.Show_Min_Order = false;
                data.Show_Max_Order = true;
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

            dataQty++;
            data.Qty++;
            
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
                prod = string.Join(",", Navbar.Filter.Select(x => x));
            } 
            else
            {
                kate = string.Join(",", Navbar.Kategori.Select(x => x.No_Id));
            }

            prod = Convert.ToBase64String(Encoding.UTF8.GetBytes(prod));
            kate = Convert.ToBase64String(Encoding.UTF8.GetBytes(kate));
            var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));

            NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}&produk={Uri.EscapeDataString(prod)}&kategori={Uri.EscapeDataString(kate)}", true);
        }

        public async void changeLogin(string value, bool direct = false)
        {
            if (InfoInitFlag == false)
            {
                await JSRuntime.InvokeVoidAsync("initButtonFlagChange");
                await JSRuntime.InvokeVoidAsync("initButtonFlag");
                InfoInitFlag = true;
            }

            if (value == "change-pw")
            {
                if (ModelRegist.NoTlp == "" || ModelRegist.NoTlp == null || ModelRegist.Password == "" || ModelRegist.Password == null)
                {
                    infoSubmit = 1;
                    StateHasChanged();
                    return;
                }

                var Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlagChange");
                if (Numberinfo == "FALSE")
                {
                    infoSubmit = 2;
                    StateHasChanged();
                    return;
                }

                var statusOtp = await JSRuntime.InvokeAsync<bool>("sendOTP", Numberinfo);

                if (statusOtp)
                {
                    await JSRuntime.InvokeVoidAsync("initLoginEffect", "otp-view", direct);
                    dataChange = 2;
                    StateHasChanged();
                    return;
                }
                else
                {
                    infoSubmit = 2;
                    StateHasChanged();
                    return;
                }
            }

            if (value == "otp-view")
            {

                if (ModelRegist.NoTlp == "" || ModelRegist.NoTlp == null || ModelRegist.Password == "" || ModelRegist.Password == null)
                {
                    infoSubmit = 1;
                    StateHasChanged();
                    return;
                }

                var Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlag");
                if (Numberinfo == "FALSE") {
                    infoSubmit = 2;
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
                        StateHasChanged();
                        return;
                    }

                    var statusOtp = await JSRuntime.InvokeAsync<bool>("sendOTP", Numberinfo);

                    if (statusOtp)
                    {
                        await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);
                        return;
                    }
                    else
                    {
                        infoSubmit = 2;
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

                await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);

            }
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

            pdjService.ShowConsoleData(Model);

            var Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlagLogin");
            if (Numberinfo == "FALSE") return;


            var param = new ParameterApi
            {
                routeApi = "auth",
                prosesApi = "login",
                ParamUrl = new ParameterApi.ParameterUrl { }
            };

            Model.NoTlp = Numberinfo;

            var loginStatus = await NewPdjService.CheckLoginApi(param, Model);

            if (loginStatus)
            {
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

            value = Regex.Replace(value, @"[^0-9]", "");

            if(info == "phone-login")
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

            Console.WriteLine(value);
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
