using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.JSInterop;
using Models;
using Radzen;
using Services;
using System.Linq;
using System.Text;

namespace PacificWeb.Components.Layout
{
    public partial class MyHeadProfile
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        private string SearchText = "";
        private bool LoadVisible = true;

        private System.Timers.Timer? _debounceTimer;
        [Parameter] public SourchData dataSourch { get; set; } = new SourchData();
        [Parameter] public string pageRoute { get; set; } = "";


        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new(), new(), 0, false);

        public List<Cart> ListDataCartCheck { get; set; } = new List<Cart>();
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public bool loadProsesNetto { get; set; } = false;
        public decimal nettoKeranjang { get; set; } = 0;
        public int searchCount { get; set; } = 0;


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
                while (dataSourch.Finised == false)
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
                                dataSourch.Finised = true;
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

        private void OnClearInput()
        {
            SearchText = "";

            SearchList = (new List<Brand>(), new List<Produk>(), 0, false);

            searchCount = 0;

            StateHasChanged();
        }

        public async Task RefreshCart()
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = dataSourch.DataUser.No_Id,
                    show_media = "true"
                }
            };
            ListDataCartCheck.Clear();
            ListDataCartCheck.AddRange(dataSourch?.ListDataCart?.FindAll(x => x.SelectCart == true));
            dataSourch.ListDataCart = await pdjService.GetCartApi(paramApi);
            if (ListDataCartCheck.Count() >= 1)
            {
                foreach (var item in dataSourch?.ListDataCart)
                {
                    if (ListDataCartCheck.Any(x => x.Id_Cart == item.Id_Cart))
                    {
                        item.SelectCart = true;
                    }
                }
            }

            StateHasChanged();
        }

        public async Task SelectItemCart(Cart cart)
        {
            loadProsesNetto = true;
            StateHasChanged();

            if (cart != null)
            {
                var data = dataSourch?.ListDataCart.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
                data.SelectCart = !data.SelectCart;

                nettoKeranjang = 0;
                foreach (var dataCart in dataSourch?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * dataCart.Harga;
                }
            } 
            else
            {
                nettoKeranjang = 0;
                foreach (var dataCart in dataSourch?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * dataCart.Harga;
                }
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
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_id = cart.Id_Cart
                }
            };
            loadProses = true;
            StateHasChanged();
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("HAPUS", cart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                await RefreshCart();
            }

            loadProses = false;
            StateHasChanged();
        }
        private async Task Increase(string id_cart)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };
            var data = dataSourch?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }
            data.Qty++;
            Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = 1, Id_Customer = dataSourch.DataUser.No_Id };
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemcart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                await RefreshCart();
            }


            nettoKeranjang = 0;
            foreach (var dataCart in dataSourch?.ListDataCart)
            {
                if (!dataCart.SelectCart) continue;
                nettoKeranjang += dataCart.Qty * dataCart.Harga;
            }

            loadProsesNetto = false;
            StateHasChanged();
        }
        private async Task Decrease(string id_cart)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            var data = dataSourch?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }
            if (data.Qty > 1)
            {
                data.Qty--;
                Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = -1, Id_Customer = dataSourch.DataUser.No_Id };
                (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemcart, paramApi);

                if (statusRespon && statusCode == 200)
                {
                    await RefreshCart();
                }

                nettoKeranjang = 0;
                foreach (var dataCart in dataSourch?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * dataCart.Harga;
                }

            }
            loadProsesNetto = false;
            StateHasChanged();
        }

        public async void NavigateToDetailProduk(string id)
        {
            List<string> stringData = await pdjService.DecodeEncode(true, new List<string> { id });
            NavigationManager.NavigateTo($"/detail-produk/{stringData.FirstOrDefault()}", forceLoad: true);
        }

        public async Task CheckoutCart()
        {

            string text = "";
            foreach (var item in dataSourch?.ListDataCart)
            {
                if (item.SelectCart) text += $"{item.Id_Cart}%section%";
            }
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/{encoded}", forceLoad: true);

        }

        private void seeResult()
        {
            var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
            NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", true);
        }

        private void searchCombo(Navbar Navbar = null, Brand brand = null, Kategori kategori = null)
        {
            var bra = "";

            var kate = "";

            if (Navbar == null)
            {
                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));
                NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand={Uri.EscapeDataString(bra)}", true);
                return;
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

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(SearchText))
            {
                var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
                NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", true);
            }
        }
    }
}
