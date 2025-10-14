using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using Services;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Security.Claims;
using static Services.pdjService;




namespace PacificWeb.Components.Pages
{
    public partial class Dashboard
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

        MyHeader header_page;
        MyFooter footer_page;

        //Header and Footer DataSource
        
        public SourchData dataSourch = new();

        public Produk DataProduk { get; set; } = new Produk();

        public bool loadProses { get; set; } = false;

        public bool loadContent { get; set; } = true;

        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }



        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                dataSourch.DataUser.Email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
                dataSourch.DataUser.No_Id = user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
            }
        }



        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                // Jalankan semua request paralel
                var produkTask = pdjService.GetProdukApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { pagesize = 12, show_variant = true, max_media = 2 , status = "aktif" }
                });

                var bannerTask = pdjService.GetBannerApi(new ParameterApi
                {
                    routeApi = "banner",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { pagesize = 3, orderBy = "nama_banner", orderDir = "desc", status = "aktif" }
                });

                var brandTask = pdjService.GetBrandApi(new ParameterApi
                {
                    routeApi = "brand",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { orderBy = "nama_brand", orderDir = "desc", status = "aktif" }
                });

                var kategoriTask = pdjService.GetKategoriApi(new ParameterApi
                {
                    routeApi = "kategori",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { pagesize = 3, status = "aktif" }
                });

                var companyTask = pdjService.GetCompanyApi(new ParameterApi
                {
                    routeApi = "company",
                    prosesApi = "get-profile",
                    ParamUrl = new ParameterApi.ParameterUrl { no_id = "comp_1" }
                });

                var CartTask = pdjService.GetCartApi(new ParameterApi
                {
                    routeApi = "cart",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = dataSourch.DataUser.No_Id,
                        show_media = "true",
                        orderBy = "Updated_At",
                        orderDir = "desc",
                        status = "aktif"
                    }
                });

                //var PesananTask = pdjService.GetPesananApi(new ParameterApi
                //{
                //    routeApi = "order",
                //    prosesApi = "getAll",
                //    ParamUrl = new ParameterApi.ParameterUrl
                //    {
                //        id_customer = dataSourch.DataUser.No_Id,
                //        show_media = "true",
                //    }
                //});

                var SummaryPesananTask = pdjService.GetSummaryPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getSummary",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = dataSourch.DataUser.No_Id,
                    }
                });

                var NavbarTask = pdjService.GetNavbarApi(new ParameterApi
                {
                    routeApi = "navbar",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = "comp_1",
                        status = "aktif"
                    }
                });

                var SosmedTask = pdjService.GetSosmedApi(new ParameterApi
                {
                    routeApi = "company",
                    prosesApi = "get-social",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = "comp_1",
                        status = "aktif"
                    }
                });


                // Tunggu semua task selesai
                await Task.WhenAll(produkTask, bannerTask, brandTask, kategoriTask, companyTask, NavbarTask, SosmedTask, CartTask, SummaryPesananTask);


                // Assign semua data sekaligus
                dataSourch.ListDataCompany = companyTask.Result;
                //dataSourch.DataPesanan = PesananTask.Result;
                dataSourch.ListDataSummaryPesanan = SummaryPesananTask.Result;
                dataSourch.ListDataNavbar = NavbarTask.Result;
                dataSourch.ListDataCompanySocial = SosmedTask.Result;
                dataSourch.ListDataCart = CartTask.Result;


                dataSourch.ListDataProduk = produkTask.Result;
                dataSourch.ListDataBanner = bannerTask.Result;
                dataSourch.ListDataBrand = brandTask.Result;
                dataSourch.ListDataKategori = kategoriTask.Result;

                dataSourch.Finised = true;
                // Render ulang satu kali setelah semua data tersedia
                StateHasChanged();

                // Jalankan JS interop setelah DOM terupdate
                if (!this.Disposed) // tambahkan flag disposed manual
                {
                    await JSRuntime.InvokeVoidAsync("initslideProduct");
                    await JSRuntime.InvokeVoidAsync("initTinySlider");
                }

                //await JSRuntime.InvokeVoidAsync("scrollToTop");
                loadContent = false;
                StateHasChanged();

                await header_page.SelectItemCart(null);
            }
        }



        public void NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}", forceLoad: true);
        }


        public void NavigateToHome(string route)
        {
            NavigationManager.NavigateTo($"/{route}", forceLoad: true);
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

            var dataProd = DataProduk?.Variants.FirstOrDefault(x => x.Stock >= DataProduk?.Min_Order && x.Status.ToLower() == "aktif");

            var Opsi = DataProduk?.Options.FirstOrDefault(x => x.Option_Id == dataProd.Option_Id);
            DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            DataProduk.Option_Default = Opsi?.Option_Label ?? "";

            var Size = DataProduk?.Sizes.FirstOrDefault(x => x.Size_Id == dataProd.Size_Id);
            DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            DataProduk.Size_Default = Size?.Size_Label ?? "";

            Quantity = DataProduk?.Min_Order ?? 0;
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

        private async Task Increase()
        {
            Quantity++;
            await QuantityChanged.InvokeAsync(Quantity);
        }

        private async Task Decrease()
        {
            if (Quantity > 1)
            {
                Quantity--;
                await QuantityChanged.InvokeAsync(Quantity);
            }
        }

        private async Task tambahKeranjang(Produk item)
        {
            loadProses = true;
            StateHasChanged();

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity, Id_Customer = dataSourch?.DataUser.No_Id  };
           (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemCart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                loadProses = false;
                await header_page.RefreshCart();
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

        public void Dispose()
        {
            Disposed = true;
        }

        private bool Disposed;
    }
}
