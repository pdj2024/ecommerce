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
        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public int QuantityProd { get; set; } = 1;
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }
        [Parameter] public EventCallback<int> QuantityProdChanged { get; set; }

        [Parameter] public string? id { get; set; }   // otomatis bind dari URL

        MyHeader header_page;
        MyFooter footer_page;

        public string ProductId => id;

        public SourchData dataSourch { get; set; } = new();

        public Produk DataProdukModal { get; set; } = new Produk();

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                var ProdukTask = pdjService.GetProdukApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        pagesize = 12,
                        show_variant = true,
                        max_media = 10,
                        no_id = ProductId
                    }
                });

                var ListProdukTask = pdjService.GetProdukApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        pagesize = 12,
                        show_variant = true,
                    }
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
                        id_customer = "cus_16",
                        show_media = "true",
                        orderBy = "Updated_At",
                        orderDir = "desc"
                    }
                });

                var PesananTask = pdjService.GetPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = "cus_16",
                        show_media = "true",
                    }
                });

                var NavbarTask = pdjService.GetNavbarApi(new ParameterApi
                {
                    routeApi = "navbar",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = "comp_1",
                    }
                });


                // Tunggu semua task selesai
                await Task.WhenAll(ProdukTask, ListProdukTask, companyTask, PesananTask, NavbarTask, CartTask);


                // Assign semua data sekaligus
                dataSourch.ListDataCompany = companyTask.Result;
                dataSourch.DataPesanan = PesananTask.Result;
                dataSourch.ListDataNavbar = NavbarTask.Result;
                dataSourch.ListDataCart = CartTask.Result;


                dataSourch.DataProduk = ProdukTask.Result.FirstOrDefault() ?? null;
                dataSourch.ListDataProduk = ListProdukTask.Result;
                dataSourch.Finised = true;

                StateHasChanged();

                if (dataSourch?.DataProduk != null && dataSourch?.DataProduk?.No_Id != "")
                {
                    Quantity = dataSourch.DataProduk.Min_Order;
                    var Opsi = dataSourch?.DataProduk.Options.FirstOrDefault();
                    dataSourch.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
                    dataSourch.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

                    var Size = dataSourch?.DataProduk.Sizes.FirstOrDefault();
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
            }



        }

        public async Task selectProduk(string id_kode)
        {
            DataProdukModal = dataSourch?.ListDataProduk.FirstOrDefault(x => x.No_Id == id_kode);
            QuantityProd = DataProdukModal.Min_Order;
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
        }

        private void OnVariantChanged(string value)
        {
            var Opsi = dataSourch?.DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            dataSourch.DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            dataSourch.DataProduk.Option_Default = Opsi?.Option_Label ?? "";

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
            var Qty = Quantity + 1;

            if (Qty <= dataSourch?.DataProduk.Min_Order)
            {
                dataSourch.DataProduk.Show_Min_Order = true;
            }
            else
            {
                Quantity++;
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
                if (Quantity <= dataSourch?.DataProduk.Min_Order)
                {
                    dataSourch.DataProduk.Show_Min_Order = true;
                }
                else
                {
                    Quantity--;
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
            loadProses = true;
            StateHasChanged();

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option_Id == item.Id_Option_Default && x.Size_Id == item.Id_Size_Default)?.No_Id ?? "";
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = via == "modal" ? QuantityProd : Quantity , Id_Customer = "cus_16" };
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

    }
}
