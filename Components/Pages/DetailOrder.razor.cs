using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;



namespace PacificWeb.Components.Pages
{
    public partial class DetailOrder 
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

        [Inject]
        public IHttpClientFactory HttpClientFactory { get; set; }

        public TextInfo ti { get; set; } = CultureInfo.CurrentCulture.TextInfo;
        public List<Produk> ListDisplayProduk { get; set; } = new List<Produk>();
        public List<Cart> ListDataCartCheck { get; set; } = new List<Cart>();

        public LoginViewModel ModelRegist { get; set; } = new();

        public int? noRegist { get; set; } = null;
        public int? noLogin { get; set; } = null;

        [Parameter] public int Quantity { get; set; } = 1;
        public decimal? nettoKeranjang { get; set; } = 0;
        public decimal? nettoKeranjangChange { get; set; } = 0;
        public decimal? totalDpp { get; set; } = 0;
        public decimal? shipingMethodFee { get; set; } = 0;
        public decimal? paymentMethodFee { get; set; } = 0;
        public decimal? ppnValue { get; set; } = 0;
        public decimal? subTotal { get; set; } = 0;
        [Parameter] public string pageTabs { get; set; } = "";
        [Parameter] public string noBukti { get; set; } = "";
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }

        public bool loadProsesNetto { get; set; } = false;

        private System.Timers.Timer? _debounceTimer;

        public bool[] PesananLoad { get; set; } = [false, false, false, false];

        public int infoSubmit { get; set; } = 0;
        public bool readOnlyProfile { get; set; } = true;
        public bool loadLogin { get; set; } = false;
        public bool updateProfil { get; set; } = false;
        public bool readOnlyAkun { get; set; } = true;
        public bool infoChange { get; set; } = false;
        public bool checkAll { get; set; } = true;
        public bool loadProses { get; set; } = false;
        public bool? infoPembatalan { get; set; } = null;
        public bool loadRefresh { get; set; } = false;
        [Parameter] public bool loadContent { get; set; } = true;
        public string Numberinfo { get; set; } = "";
        public Pesanan Pesanan_Create { get; set; } = new Pesanan();
        public Pesanan.Headers Pesanan_Change { get; set; } = new();

        MyHeader header_page;
        MyFooter footer_page;

        public SourchData dataSourch { get; set; } = new();

        public int dataQty { get; set; } = 0;
        public int dataChange { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                JSRuntime.InvokeVoidAsync("removeBackdrops");
                JSRuntime.InvokeVoidAsync("goTopInstant");

                ScopedDataServices.ScopedData.DataPesananDetail = await NewPdjService.showMorePesanan(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                        no_tracking_pesanan = noBukti,
                        show_media = "true",
                        orderDir = "desc",
                        orderBy = "no_bukti",
                        pagesize = 5,
                    }
                });

                var data = ScopedDataServices.ScopedData.DataPesananDetail.Data.Header.FirstOrDefault() ?? null;

                if (data.Metode_Bayar == "QRIS" && data.Status_Order_1 == "belum bayar")
                {
                    var base64Image = await GenerateImage(data.Payment_Number_1, data.Expiry_Time);
                    data.QrBase64 = base64Image;
                }

                loadContent = false;
                StateHasChanged();
            }

        }


        private async Task DownloadImage(Pesanan.Headers item)
        {
            var GeneratedImageBase64 = item.QrBase64;

            if (!string.IsNullOrEmpty(GeneratedImageBase64))
            {
                var base64Data = GeneratedImageBase64.Replace("data:image/png;base64,", "");
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", $"QR-PDJ-{item.No_Tracking}-{DateTime.Now.ToLocalTime()}.png", base64Data);
            }
        }


        public async Task clickCopy(Pesanan.Headers item, string init = "")
        {
            await JSRuntime.InvokeVoidAsync("copyVa", init);
        }


        private async Task<string> GenerateImage(string qrUrl, DateTime? ValidDate)
        {
            string validityText = $"Valid until {ValidDate:dd MMM yyyy - HH:mm} WIB";

            var client = HttpClientFactory.CreateClient();
            var qrBytes = await client.GetByteArrayAsync(qrUrl);

            using var qrImage = SixLabors.ImageSharp.Image.Load(qrBytes);

            // Canvas baru lebih tinggi untuk teks di atas
            int extraHeight = 30;
            int extraWidth = 40;
            int width = qrImage.Width + extraWidth;
            int height = qrImage.Height + extraHeight;
            using var finalImage = new Image<Rgba32>(width, height);

            // Gambar background putih full
            finalImage.Mutate(ctx => ctx.Fill(Color.White));

            // Area teks (rectangle putih di atas)
            finalImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White, new RectangleF(0, 0, width, extraHeight));

                var font = SystemFonts.CreateFont("Arial", 40, FontStyle.Bold);
                var textOptions = new RichTextOptions(font)
                {
                    HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
                    VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Center,
                    Origin = new PointF(width / 2f, extraHeight / 2f + 20)
                };

                ctx.DrawImage(qrImage, new SixLabors.ImageSharp.Point(0, extraHeight), 1f);
                ctx.DrawText(textOptions, validityText, Color.Purple);
            });

            // Simpan hasil ke base64
            using var ms = new MemoryStream();
            await finalImage.SaveAsPngAsync(ms);
            string base64 = Convert.ToBase64String(ms.ToArray());

            return $"data:image/png;base64,{base64}";
        }

        private async Task GantiMethod(Pesanan.Headers headItem)
        {
            Pesanan_Change = headItem;
            //await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModal");
            StateHasChanged();

            foreach(var listBayar in ScopedDataServices.ScopedData.ListDataPembayaran)
            {
                if(listBayar.Cara_Bayar == headItem.Metode_Bayar)
                {
                    listBayar.disabeldPayment = true;
                    listBayar.selectPayment = false;
                }
                else
                {
                    listBayar.disabeldPayment = false;
                    listBayar.selectPayment = false;
                }
            }

            totalDpp  = ScopedDataServices?.ScopedData?.DataPesananBelum?.Data.Items.Where(x => x.No_Bukti == headItem.No_Bukti).Sum(x => x.Harga_Satuan * x.Qty);
            shipingMethodFee  = ScopedDataServices?.ScopedData?.DataPesananBelum?.Data.Biaya.FirstOrDefault(x => x.No_Bukti == headItem.No_Bukti && x.Jenis_Item.ToLower() == "layanan")?.Harga_Satuan;
            paymentMethodFee  = ScopedDataServices?.ScopedData?.DataPesananBelum?.Data.Biaya.FirstOrDefault(x => x.No_Bukti == headItem.No_Bukti && x.Jenis_Item.ToLower() == "ongkir")?.Harga_Satuan;

            StateHasChanged();
            hitungTotal();
            StateHasChanged();
            //await JSRuntime.InvokeVoidAsync("buttonClick", $"#buttonChangeMethod");
            
        }

        private async Task BatalkanPesanan (Pesanan.Headers headItem, bool direct = false)
        {
            infoPembatalan = null;
            StateHasChanged();

            if (direct)
            {
                loadProses = true;
                var PembatalanStatus = await NewPdjService.CudPesananApi("TAMBAH", null , new ParameterApi
                {
                    routeApi = "order",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    prosesApi = $"batal/{Pesanan_Change.No_Bukti}",
                    ParamUrl = new ParameterApi.ParameterUrl()
                });

                if (PembatalanStatus.statusResponse)
                {
                    infoPembatalan = true;

                    await refreshAllPesanan();


                } else
                {
                    infoPembatalan = false;
                }

                loadProses = false;
                return;
            }


            Pesanan_Change = headItem;
            //await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModal");
            //StateHasChanged();

            
            //await JSRuntime.InvokeVoidAsync("buttonClick", $"#buttonBatalMethod");
            StateHasChanged();
        }

        public void hitungTotal()
        {
            subTotal = totalDpp + paymentMethodFee + shipingMethodFee;
            if (Pesanan_Change.Incl_PPN)
            {
                ppnValue = ((decimal)Pesanan_Change.PPN / ((decimal)100 + (decimal)Pesanan_Change.PPN)) * (decimal)subTotal;
                nettoKeranjangChange = (subTotal / ((decimal)1 + (decimal)((decimal)Pesanan_Change.PPN / (decimal)100))) + ppnValue;
            }
            else
            {
                ppnValue = ((decimal)Pesanan_Change.PPN / (decimal)100) * (decimal)subTotal;
                nettoKeranjangChange = subTotal + ppnValue;
            }
        }

        public async Task openPayment(Pesanan.Headers produk, List<Pesanan.Biayas> biaya = null)
        {
            Pesanan_Create.Data.Header.Clear();
            Pesanan_Create.Data.Biaya.Clear();
            loadRefresh = true;
            StateHasChanged();

            if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
            {
                var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                produk.QrBase64 = base64Image;
            }

            Pesanan_Create.Data.Header.Add(produk);
            Pesanan_Create.Data.Biaya.AddRange(biaya);



            if (Pesanan_Create.Data.Header.Count() >= 2) {
                var data = Pesanan_Create.Data.Header.LastOrDefault();
                Pesanan_Create.Data.Header.Remove(data);
            }

            loadRefresh = false;
            StateHasChanged();
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

        public async Task refreshAllPesanan()
        {
            var DataPesananBelum = NewPdjService.showMorePesanan(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderDir = "desc",
                    status_order_1 = "belum bayar",
                    status = "aktif",
                    orderBy = "no_bukti",
                    pagesize = 5,
                }
            });

            var DataPesananDikemas = NewPdjService.showMorePesanan(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderDir = "desc",
                    status_order_1 = "dikemas",
                    status = "aktif",
                    orderBy = "no_bukti",
                    pagesize = 5,
                }
            });

            var DataPesananDikirim = NewPdjService.showMorePesanan(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderDir = "desc",
                    status_order_1 = "dikirim",
                    status = "aktif",
                    orderBy = "no_bukti",
                    pagesize = 5,
                }
            });

            var DataPesananSelesai = NewPdjService.showMorePesanan(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderDir = "desc",
                    status_order_1 = "selesai",
                    status = "aktif",
                    orderBy = "no_bukti",
                    pagesize = 5,
                }
            });

            var DataPesananBatal = NewPdjService.showMorePesanan(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderDir = "desc",
                    status_order_1 = "batal",
                    status = "aktif",
                    orderBy = "no_bukti",
                    pagesize = 5,
                }
            });

            var SummaryPesananTask = NewPdjService.GetApi<Pesanan.SummaryResponse>(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getSummary",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                }
            });

            var dataSummary = NewPdjService.RefreshDataMainKonten();

            await Task.WhenAll(DataPesananBelum, DataPesananDikemas, DataPesananDikirim, DataPesananSelesai, dataSummary, SummaryPesananTask, DataPesananBatal);


            ScopedDataServices.ScopedData.ListDataSummaryPesanan = SummaryPesananTask.Result?.Data ?? new();
            ScopedDataServices.ScopedData.DataPesananBelum = DataPesananBelum.Result;
            ScopedDataServices.ScopedData.DataPesananDikemas = DataPesananDikemas.Result;
            ScopedDataServices.ScopedData.DataPesananDikirim = DataPesananDikirim.Result;
            ScopedDataServices.ScopedData.DataPesananSelesai = DataPesananSelesai.Result;
            ScopedDataServices.ScopedData.DataPesananBatal = DataPesananBatal.Result;
        }

    }
}
