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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PacificWeb.Components.Layout
{
    public partial class MyHeader
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
        [Parameter] public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        [Parameter] public List<Company> CompanyData { get; set; } = new List<Company>();
        public List<Produk> ListProduk { get; set; } = new List<Produk>();
        public Pesanan DataPesanan { get; set; } = new Pesanan();
        public List<Navbar> DataNavbar { get; set; } = new List<Navbar>();
        [Parameter] public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Cart> ListDataCart { get; set; } = new List<Cart>();
        public List<Cart> ListDataCartCheck { get; set; } = new List<Cart>();
        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new(), new(), 0, false);
        public bool loadProses { get; set; } = false;
        public bool loadProsesNetto { get; set; } = false;
        [Parameter]  public bool Authorize { get; set; } = false;
        public decimal nettoKeranjang { get; set; } = 0;
        public int searchCount { get; set; } = 0;

        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: true);
        }

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initMobileButton");

                while (ListDataProduk.Count() <= 0)
                {
                    await Task.Delay(500);
                }

                ListProduk = ListDataProduk.Take(3).ToList();

                ListDataCart = await pdjService.GetCartApi(new ParameterApi 
                { routeApi = "cart", 
                    prosesApi = "getAll", 
                    ParamUrl = new ParameterApi.ParameterUrl 
                    { 
                        id_customer = "cus_16",
                        show_media = "true",
                        orderBy = "Updated_At",
                        orderDir = "desc"
                    } 
                });

                DataPesanan = await pdjService.GetPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = "cus_16",
                        show_media = "true",
                    }
                });

                DataNavbar = await pdjService.GetNavbarApi(new ParameterApi
                {
                    routeApi = "navbar",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = "comp_1",
                    }
                });

                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initHeaderMega");
                await JSRuntime.InvokeVoidAsync("colLoadOut");

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
                    id_customer = "cus_16",
                    show_media = "true"
                }
            };
            ListDataCartCheck.Clear();
            ListDataCartCheck.AddRange(ListDataCart?.FindAll(x => x.SelectCart == true));
            ListDataCart = await pdjService.GetCartApi(paramApi);
            if (ListDataCartCheck.Count() >= 1)
            {
                foreach (var item in ListDataCart)
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
            var data = ListDataCart.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
            data.SelectCart = !data.SelectCart;

            nettoKeranjang = 0;
            foreach (var dataCart in ListDataCart)
            {
                if (!dataCart.SelectCart) continue;
                nettoKeranjang += dataCart.Qty * dataCart.Harga;
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
            var data = ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }
            data.Qty++;
            Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = 1, Id_Customer = "cus_16" };
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemcart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                await RefreshCart();
            }


            nettoKeranjang = 0;
            foreach (var dataCart in ListDataCart)
            {
                if (!dataCart.SelectCart) continue;
                nettoKeranjang += dataCart.Qty * dataCart.Harga;
            }

            loadProsesNetto = false;
            StateHasChanged();
        }

        public async Task CheckoutCart() {

            string text = "";
            foreach (var item in ListDataCart)
            {
                if (item.SelectCart) text += $"{item.Id_Cart}%section%";
            }
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/{encoded}", forceLoad: true);

        }

        private async Task Decrease(string id_cart)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            var data = ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }
            if (data.Qty > 1)
            {
                data.Qty--;
                Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = -1, Id_Customer = "cus_16" };
                (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemcart, paramApi);
                
                if (statusRespon && statusCode == 200)
                {
                    await RefreshCart();
                }

                nettoKeranjang = 0;
                foreach (var dataCart in ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * dataCart.Harga;
                }

            }
            loadProsesNetto = false;
            StateHasChanged();
        }

        private void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(SearchText))
            {
                var keywordText = Convert.ToBase64String(Encoding.UTF8.GetBytes(SearchText));
                NavigationManager.NavigateTo($"/search-page?keyword={Uri.EscapeDataString(keywordText)}", true);
            }
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

        private async void changeLogin(string value, bool direct = false)
        {
            await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);
        }

    }
}
