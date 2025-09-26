using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
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

        public TextInfo ti { get; set; } = CultureInfo.CurrentCulture.TextInfo;
        public List<Produk> ListDisplayProduk { get; set; } = new List<Produk>();
        public List<Cart> ListDataCartCheck { get; set; } = new List<Cart>();


        [Parameter] public int Quantity { get; set; } = 1;
        public decimal nettoKeranjang { get; set; } = 0;
        [Parameter] public string pageTabs { get; set; } = "";
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }

        public bool loadProsesNetto { get; set; } = false;

        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public Pesanan Pesanan_Create { get; set; } = new Pesanan();

        MyHeadProfile header_page;
        MyFooter footer_page;

        public SourchData dataSourch { get; set; } = new();


        protected override async Task OnInitializedAsync()
        {
            ti = CultureInfo.CurrentCulture.TextInfo;

            Pesanan_Create.Header.Add(new Pesanan.Headers());
            Pesanan_Create.Items.Add(new Pesanan.Item());
            Pesanan_Create.Biaya.Add(new Pesanan.Biayas());

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
                        orderDir = "desc"
                    }
                });

                var PesananTask = pdjService.GetPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = dataSourch.DataUser.No_Id,
                        show_media = "true",
                        orderBy = "Updated_At",
                        orderDir = "desc",
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

                var kategoriTask = pdjService.GetKategoriApi(new ParameterApi
                {
                    routeApi = "kategori",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                    }
                });


                // Tunggu semua task selesai
                await Task.WhenAll(companyTask, PesananTask, NavbarTask, CartTask);

                // Assign semua data sekaligus
                dataSourch.ListDataCompany = companyTask.Result;
                dataSourch.DataPesanan = PesananTask.Result;
                dataSourch.ListDataNavbar = NavbarTask.Result;
                dataSourch.ListDataCart = CartTask.Result;

                dataSourch.Finised = true;
                StateHasChanged();

                loadContent = false;
                StateHasChanged();
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
            dataSourch.DataPesanan = await pdjService.GetPesananApi(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = dataSourch.DataUser.No_Id,
                    show_media = "true",
                    orderBy = "no_bukti",
                    orderDir = "desc",
                }
            });


            var produk = dataSourch?.DataPesanan.Header.FirstOrDefault(x => x.No_Bukti == item.No_Bukti);

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

        public async Task deleteItemCart(Cart cart)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "delete",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_id = cart.Id_Cart
                }
            };
            loadProses = true;
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
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };
            var data = dataSourch?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }
            data.Qty++;
            Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = 1, Id_Customer = dataSourch?.DataUser.No_Id  };
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemcart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                await RefreshCart();
            }


            nettoKeranjang = 0;
            foreach (var dataCart in dataSourch?.ListDataCart)
            {
                if (!dataCart.SelectCart) continue;
                nettoKeranjang += dataCart.Qty * dataCart.Harga;
            }

            loadProsesNetto = false;
            StateHasChanged();
        }
        private async Task Decrease(string id_cart)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            var data = dataSourch?.ListDataCart.FirstOrDefault(x => x.Id_Cart == id_cart);
            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }
            if (data.Qty > 1)
            {
                data.Qty--;
                Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = -1, Id_Customer = dataSourch.DataUser.No_Id };
                (string statusMessage, bool statusRespon, int statusCode, string proses) = await pdjService.CudCartApi("TAMBAH", itemcart, paramApi);

                if (statusRespon && statusCode == 200)
                {
                    await RefreshCart();
                }

                nettoKeranjang = 0;
                foreach (var dataCart in dataSourch?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * dataCart.Harga;
                }

            }
            loadProsesNetto = false;
            StateHasChanged();
        }

        public async Task SelectItemCart(Cart cart)
        {
            loadProsesNetto = true;
            StateHasChanged();
            var data = dataSourch?.ListDataCart.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
            data.SelectCart = !data.SelectCart;

            nettoKeranjang = 0;
            foreach (var dataCart in dataSourch?.ListDataCart)
            {
                if (!dataCart.SelectCart) continue;
                nettoKeranjang += dataCart.Qty * dataCart.Harga;
            }

            loadProsesNetto = false;
            StateHasChanged();
        }

        public async Task RefreshCart()
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = dataSourch.DataUser.No_Id,
                    show_media = "true"
                }
            };
            ListDataCartCheck.Clear();
            ListDataCartCheck.AddRange(dataSourch?.ListDataCart?.FindAll(x => x.SelectCart == true));
            dataSourch.ListDataCart = await pdjService.GetCartApi(paramApi);
            if (ListDataCartCheck.Count() >= 1)
            {
                foreach (var item in dataSourch?.ListDataCart)
                {
                    if (ListDataCartCheck.Any(x => x.Id_Cart == item.Id_Cart))
                    {
                        item.SelectCart = true;
                    }
                }
            }

            StateHasChanged();
        }

        public async Task CheckoutCart()
        {

            string text = "";
            foreach (var item in dataSourch?.ListDataCart)
            {
                if (item.SelectCart) text += $"{item.Id_Cart}%section%";
            }
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/{encoded}", forceLoad: true);

        }
    }
}
