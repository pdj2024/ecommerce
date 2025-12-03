using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Services;
using System.Text;







namespace PacificWeb.Components.Sections
{
    public partial class SelectProduk : ComponentBase
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        [Inject]
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        public bool loadProses { get; set; } = false;
        public bool auth { get; set; } = false;

        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }
        [Parameter] public Produk DataProduk { get; set; } = new Produk();
        [Parameter] public MyHeader header_page { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            
            if (user == null || !user.Identity.IsAuthenticated)
            {
                auth = false;
            } else
            {
                auth = true;
            }


        }

        public void SearchCombo(string brand)
        {
            var bra = "";

            var data = ScopedDataServices?.ScopedData?.DataProduk?.Nama_Kategori.FindIndex(x => x == brand) ?? -1;

            if (data != -1)
            {
                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes( ScopedDataServices?.ScopedData?.DataProduk?.Id_Kategori[data]));
                NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(bra)}&brand=", forceLoad: false);
                return;
            }

            Console.WriteLine(data);
            pdjService.ShowConsoleData(ScopedDataServices?.ScopedData?.DataProduk);
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            
        }

        public async void NavigateToDetailProduk(string id)
        {
            List<string> stringData = await pdjService.DecodeEncode(true, new List<string> { id });
            NavigationManager.NavigateTo($"/detail-produk/{stringData.FirstOrDefault()}", forceLoad: false);
        }

        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: false);
        }

        private void OnSizeChanged(string value)
        {
            var Size = ScopedDataServices?.ScopedData?.DataProduk.Sizes.FirstOrDefault(x => x.Size_Id == value);
            ScopedDataServices.ScopedData.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Size_Default = Size?.Size_Label ?? "";

            Quantity = ScopedDataServices.ScopedData.DataProduk.Min_Order;
        }
        private void OnVariantChanged(string value)
        {
            var Opsi = ScopedDataServices?.ScopedData?.DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            ScopedDataServices.ScopedData.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            Quantity = ScopedDataServices.ScopedData.DataProduk.Min_Order;
        }

        public class SizeOption
        {
            public string Value { get; set; }
            public bool Disabled { get; set; }
            public SizeOption(string value, bool disabled = false)
            {
                Value = value;
                Disabled = disabled;
            }
        }

        private async Task Increase()
        {
            var data = ScopedDataServices?.ScopedData?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Option_Default && x.Size_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Size_Default);
            
            if (Quantity >= data?.Stock)
            {
                ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = true;
            }
            else
            {
                Quantity++;
                ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                await QuantityChanged.InvokeAsync(Quantity);
            }


            StateHasChanged();
            return;
        }

        private async Task Decrease()
        {

            if (Quantity > 1)
            {

                var data = ScopedDataServices?.ScopedData?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Option_Default && x.Size_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Size_Default);

                if (Quantity <= ScopedDataServices?.ScopedData?.DataProduk?.Min_Order)
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = true;
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                }
                else
                {
                    Quantity--;
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                    ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                    await QuantityChanged.InvokeAsync(Quantity);
                }

                StateHasChanged();
                return;
            }
        }



        private async Task tambahKeranjang(Produk item)
        {
            if (!auth) 
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(200);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#buttonLogIn");
                return;
            }

            loadProses = true;
            StateHasChanged();

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                token = ScopedDataServices.ScopedData.DataLogin?.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin?.refreshToken,
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id  };
           (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("TAMBAH", itemCart, paramApi);

            if (statusRespon && statusCode == 200 && header_page != null)
            {
                loadProses = false;
                await header_page?.RefreshCart();
                await header_page?.SelectItemCart(null);
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(200);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#openCartProd");

            } else
            {
                loadProses = false;
                StateHasChanged();
            }
        }

        //public async Task SelectItemCart(Cart cart)
        //{
        //    if (cart != null)
        //    {
        //        var data = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
        //        data.SelectCart = !data.SelectCart;

        //        nettoKeranjang = 0;
        //        foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
        //        {
        //            if (!dataCart.SelectCart) continue;
        //            nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
        //        }
        //    }
        //    else
        //    {
        //        checkAllItem(new ChangeEventArgs { Value = checkAll });
        //    }
        //}

        //public async Task refreshStatus(Pesanan.Headers item)
        //{
        //    loadProses = true;
        //    loadRefresh = true;
        //    StateHasChanged();

        //    var data = await NewPdjService.GetPesananApi(new ParameterApi
        //    {
        //        routeApi = "order",
        //        prosesApi = "getAll",
        //        token = ScopedDataServices.ScopedData.DataLogin.token,
        //        refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
        //        ParamUrl = new ParameterApi.ParameterUrl
        //        {
        //            id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
        //            no_bukti = item.No_Bukti,
        //            show_media = "true",
        //            orderBy = "no_bukti",
        //            orderDir = "desc",
        //        }
        //    });


        //    var produk = data.Data.Header.FirstOrDefault(x => x.No_Bukti == item?.No_Bukti);

        //    if (produk != null && produk?.Metode_Bayar == "QRIS" && produk?.Status_Order_1 == "belum bayar")
        //    {
        //        var base64Image = await GenerateImage(produk?.Payment_Number_1, produk?.Expiry_Time);
        //        produk.QrBase64 = base64Image;
        //    }

        //    Pesanan_Create = new();
        //    StateHasChanged();

        //    Pesanan_Create = data;

        //    if (produk?.Status_Order_1 != item?.Status_Order_1)
        //    {
        //        await JSRuntime.InvokeVoidAsync("buttonClick", $"#pills-{produk?.Status_Order_1}-tab");
        //        await refreshAllPesanan();
        //    }



        //    loadProses = false;
        //    loadRefresh = false;
        //    StateHasChanged();
        //}



        public async Task NavigateToPayment(Produk item)
        {
            if (!auth)
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");

                await Task.Delay(200);

                await JSRuntime.InvokeVoidAsync("buttonClick", "#buttonLogIn");
                //header_page.changeLogin("log-in", true);
                return;
            }

            loadProses = true;
            StateHasChanged();

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id };

            string text = $"{item.No_Id}%section%{item.Id_Variant}%section%{Quantity}";
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/direct/{encoded}", forceLoad: true);
        }

        public async Task CheckoutCart()
        {

            string text = "";
            foreach (var item in GlobalDataService?.GlobalData?.ListDataCart)
            {
                if (item.SelectCart) text += $"{item.Id_Cart}%section%";
            }
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/cart/{encoded}", forceLoad: true);

        }
    }
}
