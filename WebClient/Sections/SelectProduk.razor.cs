using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using WebClient.Layout;
using System.Text;
using WebShared.Models;
using WebShared.Services;







namespace WebClient.Sections
{
    public partial class SelectProduk : ComponentBase
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        [Inject]
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        public bool loadProses { get; set; } = false;
        public bool disabeldAdd { get; set; } = false;
        public bool auth { get; set; } = false;

        public int Quantity { get; set; } = 1;
        [Parameter] public int Quantitys { get; set; } = 1;
        [Parameter] public Produk DataProduk { get; set; } = new Produk();
        [Parameter] public MyHeader header_page { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            
            if (user == null || !user.Identity.IsAuthenticated)
            {
                auth = false;
            } 
            else
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
                NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(bra)}&brand=", forceLoad: true);
                return;
            }

        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            
        }

        public async void NavigateToDetailProduk(string id)
        {
            List<string> stringData = await pdjService.DecodeEncode(true, new List<string> { id });
            NavigationManager.NavigateTo($"/detail-produk/{stringData.FirstOrDefault()}", forceLoad: true);
        }

        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: false);
        }


        private async void OnSizeChanged(string value)
        {
            var Size = ScopedDataServices?.ScopedData?.DataProduk.Sizes.FirstOrDefault(x => x.Size_Id == value);
            ScopedDataServices.ScopedData.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Size_Default = Size?.Size_Label ?? "";

            if (NewPdjService.featurAllow.StockActive)
            {
                var data = ScopedDataServices?.ScopedData?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Option_Default && x.Size_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Size_Default);

                if (Quantity > data?.Stock)
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = true;
                } 
                else
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                }

                if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order)
                {
                    if(ScopedDataServices?.ScopedData?.DataProduk?.Min_Order >= 2)
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = true;
                    } 
                    else
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                    }
                }
                else
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                }

                if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order || Quantity > data?.Stock)
                {
                    disabeldAdd = true;
                }
                else
                {
                    disabeldAdd = false;
                }
                StateHasChanged();
            }
            else
            {
                disabeldAdd = false;
            }


        }
        private void OnVariantChanged(string value)
        {

            var Opsi = ScopedDataServices?.ScopedData?.DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            ScopedDataServices.ScopedData.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            if (NewPdjService.featurAllow.StockActive)
            {
                var data = ScopedDataServices?.ScopedData?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Option_Default && x.Size_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Size_Default);

                if (Quantity > data?.Stock)
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = true;
                } else
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                }

                if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order)
                {
                    if (ScopedDataServices?.ScopedData?.DataProduk?.Min_Order >= 2)
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = true;
                    }
                    else
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                    }
                } 
                else
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                }

                if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order || Quantity > data?.Stock)
                {
                    disabeldAdd = true;
                }
                else
                {
                    disabeldAdd = false;
                }

                StateHasChanged();
            }
            else
            {
                disabeldAdd = false;
            }


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
            
            if (NewPdjService.featurAllow.StockActive)
            {
                var data = ScopedDataServices?.ScopedData?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Option_Default && x.Size_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Size_Default);

                if (Quantity + 1 > data?.Stock)
                {
                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = true;
                }
                else
                {
                    Quantity++;

                    if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order)
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = true;
                    }
                    else
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                    }

                    ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                }


                if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order || Quantity > data?.Stock)
                {
                    disabeldAdd = true;
                }
                else
                {
                    disabeldAdd = false;
                }

            } else
            {
                Quantity++;
                //Console.WriteLine(Quantity.ToString() + "->  Baru Tambah");
            }

            StateHasChanged();
            //Console.WriteLine(Quantity.ToString() + "->  StateHasChange Akhir");
        }

        private async Task Decrease()
        {
            if (NewPdjService.featurAllow.StockActive)
            {

                var data = ScopedDataServices?.ScopedData?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Option_Default && x.Size_Id == ScopedDataServices?.ScopedData?.DataProduk?.Id_Size_Default);


                if (Quantity > 1)
                {

                    if (Quantity - 1 < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order)
                    {
                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = true;
                    }
                    else
                    {
                        Quantity--;

                        if (Quantity > data?.Stock)
                        {
                            ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = true;
                        }
                        else
                        {
                            ScopedDataServices.ScopedData.DataProduk.Show_Max_Order = false;
                        }

                        ScopedDataServices.ScopedData.DataProduk.Show_Min_Order = false;
                    }
                }


                if (Quantity < ScopedDataServices?.ScopedData?.DataProduk?.Min_Order || Quantity > data?.Stock)
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
                if (Quantity > 1)
                {
                    Quantity--;
                }
                //Console.WriteLine(Quantity.ToString() + "->  Baru Kurang");
            }


            StateHasChanged();
            //Console.WriteLine(Quantity.ToString() + "->  StateHasChange Akhir");
        }



        private async Task tambahKeranjang(Produk item)
        {
            if (NewPdjService.featurAllow.CartLogin && !auth) 
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(200);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#buttonLogIn");
                return;
            } 
            else if (!auth)
            {
                var variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default);

                if (variant == null) return;
                

                var newCart = new Cart
                {
                    Id_Produk = item.No_Id,
                    Id_Cart = $"{item.No_Id}-{variant.No_Id}",
                    Id_Variant = variant.No_Id,
                    Qty = Quantity,
                    Id_Customer = "guest"
                };

                var dataCart = await NewPdjService.GetLocalAsync<List<Cart>>("CartGuest");

                ScopedDataServices.ScopedData.ListDataCartLocal = dataCart ?? new();

                var find = ScopedDataServices.ScopedData.ListDataCartLocal.FirstOrDefault(x => x.Id_Produk == newCart.Id_Produk && x.Id_Variant == newCart.Id_Variant);

                if (find != null)
                {
                    find.Qty += Quantity;
                } 
                else
                {
                    ScopedDataServices.ScopedData.ListDataCartLocal.Add(newCart);
                }

                await header_page.AddCartLocal(item.No_Id, find ?? newCart, find != null ? true : false);

                await NewPdjService.SetLocalAsync("CartGuest", ScopedDataServices.ScopedData.ListDataCart);

                await header_page?.SelectItemCart(null);

                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(200);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#openCartProd");

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
            if (NewPdjService.featurAllow.PaymentLogin && !auth)
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
