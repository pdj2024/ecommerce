using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using Radzen.Blazor;
using Radzen;
using Services;
using System.Reflection;

using System.Configuration;

using System.Drawing;
using PacificWeb.Components.Layout;
using System.Security.Claims;
using System.Text;





namespace PacificWeb.Components.Pages
{
    public partial class DetailProduk 
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

        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public int QuantityProd { get; set; } = 1;
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }
        [Parameter] public EventCallback<int> QuantityProdChanged { get; set; }

        [Parameter] public string? id { get; set; }   // otomatis bind dari URL

        MyHeader header_page;
        MyFooter footer_page;

        public string ProductId = "";
        public bool auth = false;

        public SourchData dataSourch { get; set; } = new();

        public Produk DataProdukModal { get; set; } = new Produk();

        public Produk DataProduk { get; set; } = new Produk();

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
                        auth = true;
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
                    auth = true;
                }
            }
            else
            {
                ScopedDataServices.ScopedData.DataLogin = null;
            }


            if (id != null && id != "")
            {
                ProductId = (await pdjService.DecodeEncode(false, new List<string> { id })).FirstOrDefault();
            } 

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                var ProdukTask = NewPdjService.GetApi<Produk>(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        pagesize = 12,
                        show_variant = true,
                        max_media = 10,
                        no_id = ProductId,
                        status = "aktif"
                    }
                });

                var mainContent = NewPdjService.RefreshDataMainKonten();


                // Assign semua data sekaligus
                await Task.WhenAll(ProdukTask, mainContent);

                dataSourch.DataProduk = ProdukTask?.Result?.Data?.FirstOrDefault() ?? null;
                dataSourch.Finised = true;
                ScopedDataServices.ScopedData.Finised = true;

                StateHasChanged();

                if (dataSourch?.DataProduk != null && dataSourch?.DataProduk?.No_Id != "")
                {
                    var dataProd = dataSourch.DataProduk.Variants.FirstOrDefault(x => x.Stock >= dataSourch.DataProduk.Min_Order && x.Status.ToLower() == "aktif");

                    Quantity = dataSourch.DataProduk.Min_Order;
                    var Opsi = dataSourch?.DataProduk.Options.FirstOrDefault( x => x.Option_Id == dataProd.Option_Id);
                    dataSourch.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
                    dataSourch.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

                    var Size = dataSourch?.DataProduk.Sizes.FirstOrDefault( x => x.Size_Id == dataProd.Size_Id);
                    dataSourch.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
                    dataSourch.DataProduk.Size_Default = Size?.Size_Label ?? "";
                    
                    loadContent = false;
                    StateHasChanged();
                    await JSRuntime.InvokeVoidAsync("initFlayer");
                } 
                else
                {
                    loadContent = false;
                    StateHasChanged();
                }

                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("initslideProduct");
                await JSRuntime.InvokeVoidAsync("initBigDetailProdInfo");

                StateHasChanged();
                
                await header_page.SelectItemCart(null);
            }



        }

        public async Task clickCopy(Pesanan.Headers item, string init = "")
        {
            await JSRuntime.InvokeVoidAsync("copyVa", init);
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

            DataProdukModal = GlobalDataService.GlobalData?.ListDataProduk.FirstOrDefault(x => x.No_Id == id_kode);

            var dataProd = DataProdukModal?.Variants.FirstOrDefault(x => x.Stock >= DataProdukModal?.Min_Order && x.Status.ToLower() == "aktif");

            var Opsi = DataProdukModal?.Options.FirstOrDefault(x => x.Option_Id == dataProd.Option_Id);
            DataProdukModal.Id_Option_Default = Opsi?.Option_Id ?? "";
            DataProdukModal.Option_Default = Opsi?.Option_Label ?? "";

            var Size = DataProdukModal?.Sizes.FirstOrDefault(x => x.Size_Id == dataProd.Size_Id);
            DataProdukModal.Id_Size_Default = Size?.Size_Id ?? "";
            DataProdukModal.Size_Default = Size?.Size_Label ?? "";

            QuantityProd = DataProdukModal?.Min_Order ?? 0;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");

        }


        public void NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: true);
        }


        private void OnSizeChanged(string value)
        {
            var Size = dataSourch?.DataProduk.Sizes.FirstOrDefault(x => x.Size_Id == value);
            dataSourch.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            dataSourch.DataProduk.Size_Default = Size?.Size_Label ?? "";
            Quantity = dataSourch.DataProduk.Min_Order;
        }

        private void OnVariantChanged(string value)
        {
            var Opsi = dataSourch?.DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            dataSourch.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            dataSourch.DataProduk.Option_Default = Opsi?.Option_Label ?? "";
            Quantity = dataSourch.DataProduk.Min_Order;

        }

        private void OnSizeChangedModal(string value)
        {
            var Size = dataSourch?.DataProduk.Sizes.FirstOrDefault(x => x.Size_Id == value);
            dataSourch.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            dataSourch.DataProduk.Size_Default = Size?.Size_Label ?? "";
        }

        private void OnVariantChangedModal(string value)
        {
            var Opsi = dataSourch?.DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            dataSourch.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            dataSourch.DataProduk.Option_Default = Opsi?.Option_Label ?? "";
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
            var data = dataSourch?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == dataSourch?.DataProduk?.Id_Option_Default && x.Size_Id == dataSourch?.DataProduk?.Id_Size_Default);

            if(Quantity >= data.Stock)
            {
                dataSourch.DataProduk.Show_Min_Order = false;
                dataSourch.DataProduk.Show_Max_Order = true;
            }
            else
            {
                Quantity++;
                dataSourch.DataProduk.Show_Max_Order = false;
                dataSourch.DataProduk.Show_Min_Order = false;
                await QuantityChanged.InvokeAsync(Quantity);
            }


            StateHasChanged();
            return;
        }

        private async Task Decrease()
        {

            if (Quantity > 1)
            {

                var data = dataSourch?.DataProduk?.Variants.FirstOrDefault(x => x.Option_Id == dataSourch?.DataProduk?.Id_Option_Default && x.Size_Id == dataSourch?.DataProduk?.Id_Size_Default);

                if (Quantity <= dataSourch?.DataProduk.Min_Order)
                {
                    dataSourch.DataProduk.Show_Min_Order = true;
                    dataSourch.DataProduk.Show_Max_Order = false;
                }
                else
                {
                    Quantity--;
                    dataSourch.DataProduk.Show_Max_Order = false;
                    dataSourch.DataProduk.Show_Min_Order = false;
                    await QuantityChanged.InvokeAsync(Quantity);
                }

                StateHasChanged();
                return;
            }
        }

        private async Task IncreaseProd()
        {
            var Qty = QuantityProd + 1;

            if (Qty <= DataProdukModal.Min_Order)
            {
                DataProdukModal.Show_Min_Order = true;
            }
            else
            {
                QuantityProd++;
                DataProdukModal.Show_Min_Order = false;
                await QuantityProdChanged.InvokeAsync(QuantityProd);
            }
            StateHasChanged();
            return;
        }

        private async Task DecreaseProd()
        {
            if (QuantityProd > 1)
            {
                if (QuantityProd <= DataProdukModal.Min_Order)
                {
                    DataProdukModal.Show_Min_Order = true;
                } else
                {
                    QuantityProd--;
                    DataProdukModal.Show_Min_Order = false;
                    await QuantityProdChanged.InvokeAsync(QuantityProd);
                }
                StateHasChanged();
                return;
            }
        }

        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: true);
        }

        private async Task tambahKeranjang(Produk item, string via)
        {
            if (!auth)
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");

                await Task.Delay(200);

                await JSRuntime.InvokeVoidAsync("buttonClick", "#buttonLogIn");
                header_page.changeLogin("log-in", true);
                return;
            }

            loadProses = true;
            StateHasChanged();

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl {  }
            };

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = via == "modal" ? QuantityProd : Quantity , Id_Customer = ScopedDataServices.ScopedData.DataLogin.No_Id  };
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemCart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                loadProses = false;
                await header_page.RefreshCart();
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(200);
                await JSRuntime.InvokeVoidAsync("buttonClick", "#openCartProd");
            }
            else
            {
                loadProses = false;
                StateHasChanged();
            }
        }


        public async Task NavigateToPayment(Produk item)
        {
            if (ScopedDataServices.ScopedData.DataLogin.No_Id == null || ScopedDataServices.ScopedData.DataLogin.No_Id == "")
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");

                await Task.Delay(200);

                await JSRuntime.InvokeVoidAsync("buttonClick", "#buttonLogIn");
                header_page.changeLogin("log-in", true);
                return;
            }

            loadProses = true;
            StateHasChanged();

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity, Id_Customer = ScopedDataServices.ScopedData.DataLogin.No_Id };

            string text = $"{item.No_Id}%section%{item.Id_Variant}%section%{Quantity}";
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/direct/{encoded}", forceLoad: true);
        }
    }
}
