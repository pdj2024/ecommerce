using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Models;
using Radzen;
using Radzen.Blazor;
using Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static Models.Pesanan;








namespace PacificWeb.Components.Pages
{
    public partial class PaymentPage
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

        [Parameter]
        public string? CartList { get; set; }   // otomatis bind dari URL

        [Parameter]
        public string? itemVariant { get; set; }   // otomatis bind dari URL

        public TextInfo ti { get; set; } = CultureInfo.CurrentCulture.TextInfo;
        public string listCard { get; set; }
        public string[] listDirectItem { get; set; }
        public Pesanan Pesanan_Info { get; set; } = new Pesanan();
        public Pesanan Pesanan_Create { get; set; } = new Pesanan();
        public Pengiriman dataKirim { get; set; } = new Pengiriman();
        public Pembayaran dataLayanan { get; set; } = new Pembayaran();
        public SourchData dataSourch { get; set; } = new();
        public decimal nettoKeranjang { get; set; } = 0;
        public decimal paymentMethodFee { get; set; } = 0;
        public decimal shipingMethodFee { get; set; } = 0;
        public int countRedirect { get; set; } = 5;
        public string cara_bayar { get; set; } = "";
        public bool alertPayment { get; set; } = false;
        public bool alertProfile { get; set; } = false;
        public bool alertCreate { get; set; } = false;
        public bool loadProses { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public bool loadRefresh { get; set; } = true;
        protected override async Task OnInitializedAsync()
        {

            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                dataSourch.DataUser.Email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
                dataSourch.DataUser.No_Id = user.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
            }
            else
            {
                NavigationManager.NavigateTo("/", true);
            }


            if (CartList == "cart")
            {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(itemVariant));
                listCard = decoded.Replace("%section%", ",");
            }
            else if (CartList == "direct") {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(itemVariant));
                listDirectItem = decoded.Split("%section%");
            }
            else
            {
                NavigationManager.NavigateTo("/", true);
            }

                Pesanan_Info.Header.Add(new Pesanan.Headers());
                Pesanan_Info.Items.Add(new Pesanan.Item());
                Pesanan_Info.Biaya.Add(new Pesanan.Biayas());
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

                var metodeBayarTask = pdjService.GetMetodeBayarApi(new ParameterApi
                {
                    routeApi = "payment",
                    prosesApi = "option",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        orderBy = "fee",
                        orderDir = "asc",
                    }
                });

                var metodeKirimTask = pdjService.GetMetodeKirimApi(new ParameterApi
                {
                    routeApi = "shipping",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        orderBy = "fee",
                        orderDir = "asc",
                    }
                });


                if (CartList == "cart")
                {
                    var dataCard = await pdjService.GetCartApi(new ParameterApi
                    {
                        routeApi = "cart",
                        prosesApi = "getAll",
                        ParamUrl = new ParameterApi.ParameterUrl
                        {
                            noid_list = listCard,
                            show_media = "true"
                        }
                    });

                    if (dataCard.Count() >= 1)
                    {
                        dataSourch.ListDataCart.AddRange(dataCard);
                        
                    } else
                    {
                        await pdjService.ButtonCLick("modalPaymentStatus4");
                        await Task.Delay(5000);
                        await pdjService.GoBack();
                    }

                }
                else
                {
                    var dataDirect = await pdjService.GetProdukApi(new ParameterApi
                    {
                        routeApi = "produk",
                        prosesApi = "getAll",
                        ParamUrl = new ParameterApi.ParameterUrl
                        {
                            no_id = listDirectItem[0],
                            show_media = "true",
                            show_variant = true
                        }
                    });

                    var itemDirect = dataDirect.FirstOrDefault(x => x.No_Id == listDirectItem[0])?.Variants.FirstOrDefault(x => x.No_Id == listDirectItem[1]);
                    var itemProduct = dataDirect.FirstOrDefault(x => x.No_Id == listDirectItem[0]);

                    if (itemDirect != null && itemDirect.Stock >= int.Parse(listDirectItem[2]))
                    {
                        dataSourch.ListDataCart.Add(new Cart 
                        { 
                            Id_Variant = itemDirect.No_Id, 
                            Qty = int.Parse(listDirectItem[2]), 
                            Harga = itemDirect.Harga,
                            Size = itemDirect?.Size_Id,
                            Option = itemDirect?.Option_Id,
                            Nama_Display = itemProduct?.Nama_Display,
                            Stock = itemDirect.Stock,
                            Id_Media = dataDirect.FirstOrDefault(x => x.No_Id == listDirectItem[0])?.Options.FirstOrDefault(x => x.Option_Id == itemDirect.Option_Id)?.MediaUrl.Id_Media}
                        );
                        Console.WriteLine("add cart");
                    }
                    else if (itemDirect != null && itemDirect.Stock < int.Parse(listDirectItem[2]))
                    {
                        await pdjService.ButtonCLick("modalPaymentStatus3");
                        Console.WriteLine("stock tidak cukup");
                        //await Task.Delay(3000);
                        //await pdjService.GoBack();
                    } else
                    {
                        await pdjService.ButtonCLick("modalPaymentStatus4");
                        Console.WriteLine("rusak");
                    }


                }


                // Tunggu semua task selesai
                await Task.WhenAll(companyTask, metodeBayarTask, metodeKirimTask);

                // Assign semua data sekaligus
                dataSourch.ListDataCompany = companyTask.Result;
                dataSourch.ListDataPembayaran = metodeBayarTask.Result;
                dataSourch.ListDataPengiriman = metodeKirimTask.Result;



                nettoKeranjang = dataSourch.ListDataCart.Sum(x => x.Harga * x.Qty);
                loadContent = false;
                StateHasChanged();

            }
        }

        public async Task SelectPaymentMethod(Pembayaran payment)
        {
            var active = dataSourch?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);
            var method = dataSourch?.ListDataPembayaran.FirstOrDefault(x => x.Cara_Bayar == payment.Cara_Bayar);

            if (active == null)
            {
                method.selectPayment = true;
            }
            else
            {
                active.selectPayment = false;
                method.selectPayment = true;
            }

            paymentMethodFee = method.Fee;
            cara_bayar = method.Cara_Bayar.Split(" ")[0];
            dataLayanan = method;
            nettoKeranjang = dataSourch.ListDataCart.Sum(x => x.Harga * x.Qty) + paymentMethodFee + shipingMethodFee;
            StateHasChanged();
        }

        public async Task SelectShipingMethod(Pengiriman shipping)
        {
            var active = dataSourch?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
            var method = dataSourch?.ListDataPengiriman.FirstOrDefault(x => x.No_Id == shipping.No_Id);

            if (active == null)
            {
                method.selectShipping = true;
            }
            else
            {
                active.selectShipping = false;
                method.selectShipping = true;
            }

            shipingMethodFee = shipping.Fee;
            dataKirim = shipping;
            nettoKeranjang = dataSourch.ListDataCart.Sum(x => x.Harga * x.Qty) + paymentMethodFee + shipingMethodFee;
            StateHasChanged();
        }

        public async Task NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}");
        }

        public async Task FinisedOrder()
        {

            dataKirim = dataSourch?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
            dataLayanan = dataSourch?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);

            if (dataKirim == null || dataLayanan == null)
            {
                alertPayment = true;
            }
            else
            {
                alertPayment = false;
            }

            if (Pesanan_Info.Header[0].Nama_Penerima == "" || Pesanan_Info.Header[0].Telpon_Penerima == "" || Pesanan_Info.Header[0].Alamat_Kirim == "")
            {
                alertProfile = true;
            }
            else
            {
                alertProfile = false;
            }

            if (alertPayment || alertProfile)
            {
                return;
            }
            else
            {

            }

            await JSRuntime.InvokeVoidAsync("buttonClick", "#modalPaymentStatus");
            await JSRuntime.InvokeVoidAsync("initVaCopy");
        }


        public async Task clickCopy(Pesanan.Headers item, string init = "")
        {
            await JSRuntime.InvokeVoidAsync("copyVa", init);
        }


        public async Task directStatus(Pesanan.Headers item)
        {
            var dataUrl = item.Status_Order_1 == "belum bayar" ? "belum-bayar" : item.Status_Order_1;
            NavigationManager.NavigateTo($"/cart-page/{dataUrl}",  true);
        }


        public async Task refreshStatus(Pesanan.Headers item)
        {
            loadProses = true;
            loadRefresh = true;

            StateHasChanged();

            var dataOrder = await pdjService.GetPesananApi(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_bukti = item.No_Bukti
                }
            });

            Pesanan_Create.Header.Clear();
            StateHasChanged();


            foreach (var produk in dataOrder.Header)
            {
                if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
                {
                    var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                    produk.QrBase64 = base64Image;
                }

                Pesanan_Create.Header.Add(produk);
                StateHasChanged();
            }

            StateHasChanged();



            loadProses = false;
            loadRefresh = false;
            StateHasChanged();

            if (Pesanan_Create.Header.FirstOrDefault()?.Status_Order_1 != "belum bayar")
            {
                while (countRedirect != 0)
                {
                    await Task.Delay(1000);
                    countRedirect--;
                    StateHasChanged();
                }

                await directStatus(Pesanan_Create.Header.FirstOrDefault());
            }

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
        public async Task PayOrder()
        {
            loadProses = true;
            loadRefresh = true;
            StateHasChanged();
            dataKirim = dataSourch?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
            dataLayanan = dataSourch?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);

            List<object> ListDataItem = new List<object>();
            List<object> ListDataBiaya = new List<object>();


            foreach (var item in dataSourch.ListDataCart)
            {
                item.Jenis_Item = "stock";
                item.Harga_Satuan = item.Harga;

                var data_item = new
                {
                    Jenis_Item = item.Jenis_Item,
                    Id_Variant = item.Id_Variant,
                    Qty = item.Qty,
                    Harga_Satuan = item.Harga
                };

                ListDataItem.Add(data_item);
            }

            List<object> Layanan = new List<object>
            {
                new
                {
                    Jenis_Item = "ongkir",
                    Id_Variant = "ONGKIR",
                    Qty = 1,
                    Harga_Satuan = dataKirim.Fee
                },
                new
                {
                    Jenis_Item = "layanan",
                    Id_Variant = "Layanan",
                    Qty = 1,
                    Harga_Satuan = dataLayanan.Fee
                }
            };

            var dataOrderCart = new
            {
                Id_Customer = dataSourch?.DataUser.No_Id,
                Status_Bayar = "",
                Id_Payment_Gateway = "",
                Payment_Type = dataLayanan.Cara_Bayar,
                Nama_Penerima = Pesanan_Info.Header[0].Nama_Penerima,
                Telpon_Penerima = Pesanan_Info.Header[0].Telpon_Penerima,
                Alamat_Kirim = Pesanan_Info.Header[0].Alamat_Kirim,
                Id_Shipping_Option = dataKirim.No_Id,
                Items = ListDataItem,
                Biaya = Layanan,
                ambil_dari_cart = CartList == "cart" ? true : false
            };


            var dataCard = await pdjService.CudPesananApi("TAMBAH", dataOrderCart, new ParameterApi
            {
                routeApi = "order",
                prosesApi = "create",
                ParamUrl = new ParameterApi.ParameterUrl()
            });



            var data = dataCard.messageResponse.Split("%section%");

            if (dataCard.statusResponse)
            {
                var noBukti = data.Count() > 1 ? data[1] : "";

                var dataOrder = await pdjService.GetPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    { 
                        no_bukti = noBukti
                    }
                });

                Pesanan_Create.Header.Clear();
                StateHasChanged();

                foreach (var produk in dataOrder.Header)
                {
                    if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
                    {
                        var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                        produk.QrBase64 = base64Image;
                    }

                    Pesanan_Create.Header.Add(produk);
                    StateHasChanged();
                }



                loadProses = false;
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#modalPaymentStatus2");

                if (Pesanan_Create.Header.FirstOrDefault() == null) { 
                    loadRefresh = true;
                    Pesanan_Create.Header.Add(new Headers { No_Bukti = noBukti });
                } 
                else {
                    loadRefresh = false;
                }

            } else
            {
                alertCreate = true;
                loadProses = false; 
                StateHasChanged();
                return;
            }

        }
    }
}
