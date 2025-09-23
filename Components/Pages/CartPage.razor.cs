using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace PacificWeb.Components.Pages
{
    public partial class CartPage 
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
        public IHttpClientFactory HttpClientFactory { get; set; }

        public List<Pembayaran> ListDataDb { get; set; } = new List<Pembayaran>();
        public TextInfo ti { get; set; } = CultureInfo.CurrentCulture.TextInfo;
        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public Produk DataProduk { get; set; } = new Produk();
        public List<Pesanan> ListDataPesanan { get; set; } = new List<Pesanan>();
        public List<Cart> ListDataCart { get; set; } = new List<Cart>();
        public Pesanan DataPesanan { get; set; } = new Pesanan();
        public List<Produk> ListDisplayProduk { get; set; } = new List<Produk>();
        public List<Pembayaran> ListData { get; set; } = new List<Pembayaran>();
        [Parameter] public int Quantity { get; set; } = 1;
        [Parameter] public string pageTabs { get; set; } = "";
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }
        public bool loadProses { get; set; } = false;
        public Pesanan Pesanan_Create { get; set; } = new Pesanan();
        MyHeadProfile header_page;
        MyFooter footer_page;

        protected override async Task OnInitializedAsync()
        {
            ti = CultureInfo.CurrentCulture.TextInfo;

            Pesanan_Create.Header.Add(new Pesanan.Headers());
            Pesanan_Create.Items.Add(new Pesanan.Item());
            Pesanan_Create.Biaya.Add(new Pesanan.Biayas());
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ListDataDb = await pdjService.GetMetodeBayarApi(new ParameterApi
                {
                    routeApi = "payment",
                    prosesApi = "option",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        orderBy = "fee",
                        orderDir = "desc",
                    }
                });

                ListDataProduk = await pdjService.GetProdukApi(new ParameterApi
                {
                    routeApi = "produk",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        pagesize = 8,
                        show_variant = true,
                    }
                });

                ListDataCart = await pdjService.GetCartApi(new ParameterApi
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

                DataPesanan = await pdjService.GetPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = "cus_16",
                        show_media = "true",
                        orderBy = "Updated_At",
                        orderDir = "desc",
                    }
                });

                ListData.AddRange(ListDataDb);
                await JSRuntime.InvokeVoidAsync("initslideProduct");
                await JSRuntime.InvokeVoidAsync("scrollToTop");
                StateHasChanged();

                StateHasChanged();
                await Task.Delay(500);
                await JSRuntime.InvokeVoidAsync("fadeout");
            }

        }

        public async Task redirectWa(Pesanan.Headers item)
        {
            var phone = "081918155567"; // Nomor tujuan
            var message = "Halo, saya mau pesan produk Anda";
            var url = $"https://wa.me/${phone}?text={message}";
            //NavigationManager.NavigateTo(url, true, true);
        }


        public async Task refreshStatus(Pesanan.Headers item)
        {
            loadProses = true;
            StateHasChanged();
            DataPesanan = await pdjService.GetPesananApi(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = "cus_16",
                    show_media = "true",
                    orderBy = "no_bukti",
                    orderDir = "desc",
                }
            });


            var produk = DataPesanan.Header.FirstOrDefault(x => x.No_Bukti == item.No_Bukti);

            if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
            {
                var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                produk.QrBase64 = base64Image;
            }

            Pesanan_Create.Header.Clear();
            StateHasChanged();

            Pesanan_Create.Header.Add(produk);

            loadProses = false;
            StateHasChanged();
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


        private async Task DownloadImage(Pesanan.Headers item)
        {
            var GeneratedImageBase64 = item.QrBase64;

            if (!string.IsNullOrEmpty(GeneratedImageBase64))
            {
                var base64Data = GeneratedImageBase64.Replace("data:image/png;base64,", "");
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", $"QR-PDJ-{item.No_Bukti}-{DateTime.Now.ToLocalTime()}.png", base64Data);
            }
        }


        public async Task clickCopy(Pesanan.Headers item, string init = "")
        {
           
            await JSRuntime.InvokeVoidAsync("copyVa", item.No_Bukti + init);
        }

        public async Task NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}");
        }


        public async void selectProduk(string id_kode)
        {
            DataProduk = ListDataProduk.FirstOrDefault(x => x.No_Id == id_kode);
            Quantity = 1;
            StateHasChanged();
            await JSRuntime.InvokeVoidAsync("initSearchButton");
        }

        public async void openDropdown(string id_kode)
        {
            await JSRuntime.InvokeVoidAsync("initItemsOrder", id_kode);
        }

        public async Task openPayment(Pesanan.Headers produk)
        {
            Pesanan_Create.Header.Clear(); 
            StateHasChanged();

            if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
            {
                var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                produk.QrBase64 = base64Image;
            }

            Pesanan_Create.Header.Add(produk);
            StateHasChanged();
        }

        private void OnSizeChanged(string value)
        {
            var Size = DataProduk.Sizes.FirstOrDefault(x => x.Size_Id == value);
            DataProduk.Id_Size_Default = Size?.Size_Id ?? "";
            DataProduk.Size_Default = Size?.Size_Label ?? "";
        }
        private void OnVariantChanged(string value)
        {
            var Opsi = DataProduk.Options.FirstOrDefault(x => x.Option_Id == value);
            DataProduk.Id_Option_Default = Opsi?.Option_Id ?? "";
            DataProduk.Option_Default = Opsi?.Option_Label ?? "";
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

        private async Task showPayment(Pesanan.Headers data)
        {
            Pesanan_Create.Header.Add(data);
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
            Cart itemCart = new Cart { Id_Variant = item.Id_Variant, Qty = Quantity };
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemCart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                loadProses = false;
                await header_page.RefreshCart();
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#closeDetailProd");
                await Task.Delay(1000);
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
