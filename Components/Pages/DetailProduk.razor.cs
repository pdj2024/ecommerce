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
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }
        [Parameter] public EventCallback<int> QuantityProdChanged { get; set; }

        [Parameter] public string? id { get; set; }   // otomatis bind dari URL

        MyHeader header_page;
        MyFooter footer_page;

        public string ProductId => id;

        public List<Produk> ListDataProdukApi { get; set; } = new List<Produk>();
        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public Produk DataProduk { get; set; } = new Produk();
        public Produk DataProdukModal { get; set; } = new Produk();

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                ListDataProdukApi = await pdjService.GetProdukApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        pagesize = 12,
                        show_variant = true,
                    }
                });
                ListDataProduk.AddRange(ListDataProdukApi);
                ListDataKategori = await pdjService.GetKategoriApi(new ParameterApi
                {
                    routeApi = "kategori",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                    }
                });
                var data = ListDataProdukApi.FirstOrDefault(x => x.No_Id == ProductId);
                DataProduk = data != null || ListDataProdukApi.Count >= 1 ? data : new Produk();
                Quantity = DataProduk.Min_Order;
                DataProduk.Option = DataProduk.Options.FirstOrDefault()?.Option;

                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initslideProduct");
                await JSRuntime.InvokeVoidAsync("initFlayer");
                await JSRuntime.InvokeVoidAsync("scrollToTop");
                //await JSRuntime.InvokeVoidAsync("statusBarHidden");
                await JSRuntime.InvokeVoidAsync("initBigDetailProdInfo");


                StateHasChanged();
                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("fadeout");

            }



        }

        public async Task selectProduk(string id_kode)
        {
            DataProdukModal = ListDataProduk.FirstOrDefault(x => x.No_Id == id_kode);
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
            DataProduk.Size_Default = value;
        }

        private void OnVariantChanged(string value)
        {
            DataProduk.Option = value;
        }

        private void OnSizeChangedModal(string value)
        {
            DataProdukModal.Size_Default = value;
        }

        private void OnVariantChangedModal(string value)
        {
            DataProdukModal.Option = value;
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

            if (Qty <= DataProduk.Min_Order)
            {
                DataProduk.Show_Min_Order = true;
            }
            else
            {
                Quantity++;
                DataProduk.Show_Min_Order = false;
                await QuantityChanged.InvokeAsync(Quantity);
            }
            StateHasChanged();
            return;
        }

        private async Task Decrease()
        {

            if (Quantity > 1)
            {
                if (Quantity <= DataProduk.Min_Order)
                {
                    DataProduk.Show_Min_Order = true;
                }
                else
                {
                    Quantity--;
                    DataProduk.Show_Min_Order = false;
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

            item.Id_Variant = item.Variants.FirstOrDefault(x => x.Option == item.Option && x.Size == item.Size_Default)?.No_Id ?? "";
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
