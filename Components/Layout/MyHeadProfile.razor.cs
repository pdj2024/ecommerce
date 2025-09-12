using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
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
        [Parameter] public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        [Parameter] public string pageRoute { get; set; } = "";
        public List<Produk> ListProduk { get; set; } = new List<Produk>();
        [Parameter] public Pesanan DataPesanan { get; set; } = new Pesanan();
        public List<Cart> ListDataCart { get; set; } = new List<Cart>();
        public List<Cart> ListDataCartCheck { get; set; } = new List<Cart>();
        public bool loadProses { get; set; } = false;
        public bool loadProsesNetto { get; set; } = false;
        public decimal nettoKeranjang { get; set; } = 0;

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
                ParameterApi paramApi = new ParameterApi
                {
                    routeApi = "cart",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = "cus_16",
                        show_variant = true
                    }
                };
                await JSRuntime.InvokeVoidAsync("initMobileButton");
                await JSRuntime.InvokeVoidAsync("initHeaderMega");

                while (DataPesanan.Header.Count() <= 0)
                {
                    await Task.Delay(500);
                }

                ListProduk = ListDataProduk.Take(3).ToList();
                ListDataCart = await pdjService.GetCartApi(paramApi);
                StateHasChanged();
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
                    await SearchDataAsync(SearchText);
                    StateHasChanged();
                });
            };
            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();
        }

        private Task SearchDataAsync(string? query)
        {
            Console.WriteLine($"🔎 Search dipanggil: {query}");
            return Task.CompletedTask;
        }

        private void OnClearInput()
        {
            SearchText = "";
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
                    show_variant = true
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
            loadProses = true;
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "delete",
                ParamUrl = new ParameterApi.ParameterUrl()
            };
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
            var data = ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

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
            var data = ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };
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
    }
}
