using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Services;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;




namespace PacificWeb.Components.Pages
{
    public partial class DashboardTest
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


        MyHeader header_page;
        MyFooter footer_page;

        //Header and Footer DataSource
        
        public Produk DataProduk { get; set; } = new Produk();

        public bool loadProses { get; set; } = true;

        public bool loadContent { get; set; } = true;

        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }



        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                var Phone = user.FindFirst(c => c.Type == ClaimTypes.OtherPhone)?.Value;
                var No_Id = user.FindFirst(c => c.Type == ClaimTypes.Sid)?.Value;
                var Password = user.FindFirst(c => c.Type == ClaimTypes.PrimarySid)?.Value;
                var Nama_display = user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;

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
                var companyTask = NewPdjService.GetListGlobalDataAsync<Company>(new ParameterApi
                {
                    routeApi = "company",
                    prosesApi = "get-profile",
                    ParamUrl = new ParameterApi.ParameterUrl { no_id = "comp_1" }
                });


                var NavbarTask = NewPdjService.GetListGlobalDataAsync<Navbar>(new ParameterApi
                {
                    routeApi = "navbar",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = "comp_1",
                        status = "aktif"
                    }
                });

                var SosmedTask = NewPdjService.GetListGlobalDataAsync<CompanySosial>(new ParameterApi
                {
                    routeApi = "company",
                    prosesApi = "get-social",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = "comp_1",
                        status = "aktif"
                    }
                });

                var TeksTask = NewPdjService.GetListGlobalDataAsync<TextSection>(new ParameterApi
                {
                    routeApi = "teks",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { }
                });

                var bannerTask = NewPdjService.GetListGlobalDataAsync<Produk>(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { pagesize = 12, show_variant = true, max_media = 2, status = "aktif", orderBy = "tgl_created", orderDir = "desc" }
                });

                var authContent = NewPdjService.RefreshDataAuthKonten();

                // Tunggu semua task selesai
                await Task.WhenAll(companyTask, NavbarTask, SosmedTask, TeksTask, authContent, bannerTask);

                await header_page.SelectItemCart(null);

                //await JSRuntime.InvokeVoidAsync("initTinySlider");
                await JSRuntime.InvokeVoidAsync("colLoadOut");
                await JSRuntime.InvokeVoidAsync("initProduk");
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("initslideHomeProduct");
                StateHasChanged();
                
            }
        }



        public void NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: false);
        }


        public void SearchCombo(Brand brand)
        {
            var bra = "";
            if (brand != null)
            {
                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));
                NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori=&brand={Uri.EscapeDataString(bra)}", false);
                return;
            }
        }


        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: false);
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

            var dataProd = ScopedDataServices.ScopedData.DataProduk?.Variants.FirstOrDefault(x => x.Stock >= ScopedDataServices.ScopedData.DataProduk?.Min_Order && x.Status.ToLower() == "aktif");

            var Opsi = ScopedDataServices.ScopedData.DataProduk?.Options.FirstOrDefault(x => x.Option_Id == (dataProd?.Option_Id ?? ""));
            ScopedDataServices.ScopedData.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            var Size = ScopedDataServices.ScopedData.DataProduk?.Sizes.FirstOrDefault(x => x.Size_Id == (dataProd?.Size_Id ?? ""));
            ScopedDataServices.ScopedData.DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            ScopedDataServices.ScopedData.DataProduk.Size_Default = Size?.Size_Label ?? "";

            Quantity = ScopedDataServices.ScopedData.DataProduk?.Min_Order ?? 0;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");

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

        public void Dispose()
        {
            Disposed = true;
        }

        private bool Disposed;
    }
}
