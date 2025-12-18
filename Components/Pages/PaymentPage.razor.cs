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
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

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
        public decimal totalDpp { get; set; } = 0;
        public decimal ppnValue { get; set; } = 0;
        public decimal subtotal { get; set; } = 0;
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
        public bool contentCrash { get; set; } = true;





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
                NavigationManager.NavigateTo("/", false);
            }

            try 
            {
                if (CartList == "cart")
                {
                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(itemVariant));
                    listCard = decoded.Replace("%section%", ",");
                }
                if (CartList == "repurchase")
                {
                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(itemVariant));
                    listCard = decoded;
                }
                else if (CartList == "direct") {
                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(itemVariant));
                    listDirectItem = decoded.Split("%section%");
                }
                else
                {
                    NavigationManager.NavigateTo("/", false);
                }
            } 

            catch 
            {
                contentCrash = false;
            }

            Pesanan_Info.Data.Header.Add(new Pesanan.Headers());
            Pesanan_Info.Data.Items.Add(new Pesanan.Item());
            Pesanan_Info.Data.Biaya.Add(new Pesanan.Biayas());
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try 
                {
                    JSRuntime.InvokeVoidAsync("goTopInstant");
                    if (CartList == "cart")
                    {
                        var dataCard = await NewPdjService.GetApi<Cart>(new ParameterApi
                        {
                            routeApi = "cart",
                            prosesApi = "getAll",
                            token = ScopedDataServices.ScopedData.DataLogin.token,
                            refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                            ParamUrl = new ParameterApi.ParameterUrl
                            {
                                noid_list = listCard,
                                show_media = "true",
                                status = "aktif",
                            }
                        });

                        if(dataCard?.Data?.FirstOrDefault(x => x.Stock < x.Qty) != null)
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus3");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                            return;
                        }

                        if (dataCard != null && dataCard?.Data.Count() >= 1)
                        {
                            dataSourch.ListDataCart.AddRange(dataCard.Data);

                        }
                        else
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus4");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                        }

                    }
                    if (CartList == "repurchase")
                    {
                        var dataPesanan = await NewPdjService.showMorePesanan(new ParameterApi
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
                                orderBy = "no_bukti",
                                pagesize = 5,
                                no_tracking_pesanan = listCard,
                            }
                        });

                        if (dataPesanan?.Data?.Header?.FirstOrDefault( x => x.No_Tracking == listCard) == null)
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus6");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                            return;
                        }

                        if (dataPesanan?.Data?.Items?.Count(x => x.Var_Stock < x.Qty || x.Var_Status != "aktif") == dataPesanan?.Data?.Items?.Count())
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus3");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                            return;
                        }


                        foreach (var itemDirect in dataPesanan.Data.Items)
                        {

                            if (itemDirect.Var_Stock < itemDirect.Qty || itemDirect.Var_Status != "aktif") continue;

                            dataSourch.ListDataCart.Add(new Cart
                            {
                                Id_Variant = itemDirect.Id_Variant,
                                Qty = itemDirect.Qty,
                                Harga = itemDirect.Var_Harga,
                                Size_Id = itemDirect?.Var_Size,
                                Size_Label = itemDirect?.Var_Size_Label,
                                Option_Id = itemDirect?.Var_Option,
                                Option_Label = itemDirect?.Var_Option_Label,
                                Diskon1 = itemDirect.Diskon1,
                                Diskon2 = itemDirect.Diskon2,
                                Nama_Display = itemDirect?.Nama_Display,
                                Stock = itemDirect.Var_Stock,
                                Id_Media = itemDirect.Var_Id_Media,
                            });
                        }

                        if (dataPesanan?.Data?.Items?.FirstOrDefault(x => x.Var_Stock < x.Qty || x.Var_Status != "aktif") != null)
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus7");
                        }

                    }
                    else
                    {
                        if (listDirectItem.Count() <= 2)
                        {
                            contentCrash = false;
                        }

                        var dataDirect = await NewPdjService.GetApi<Produk>(new ParameterApi
                        {
                            routeApi = "produk",
                            prosesApi = "getAll",
                            ParamUrl = new ParameterApi.ParameterUrl
                            {
                                status = "aktif",
                                no_id = listDirectItem[0],
                                show_media = "true",
                                show_variant = true
                            }
                        });

                        var itemProduct = dataDirect.Data.FirstOrDefault(x => x.No_Id == listDirectItem[0]);
                        var itemDirect = dataDirect.Data.FirstOrDefault(x => x.No_Id == listDirectItem[0])?.Variants.FirstOrDefault(x => x.No_Id == listDirectItem[1]);

                        if (itemProduct == null || itemDirect.Stock < int.Parse(listDirectItem[2]) || itemDirect.Status != "aktif" || itemProduct.Status != "aktif")
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus8");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                            return;
                        }

                        if (itemDirect != null && itemDirect.Stock >= int.Parse(listDirectItem[2]))
                        {
                            dataSourch.ListDataCart.Add(new Cart
                            {
                                Id_Variant = itemDirect.No_Id,
                                Qty = int.Parse(listDirectItem[2] ?? "0"),
                                Harga = itemDirect.Harga,
                                Size_Id = itemDirect?.Size_Id,
                                Size_Label = itemDirect?.Size_Label,
                                Option_Id = itemDirect?.Option_Id,
                                Option_Label = itemDirect?.Option_Label,
                                Diskon1 = itemProduct?.Diskon1,
                                Diskon2 = itemProduct?.Diskon2,
                                Nama_Display = itemProduct?.Nama_Display,
                                Stock = itemDirect.Stock,
                                Id_Media = dataDirect.Data.FirstOrDefault(x => x.No_Id == listDirectItem[0])?.Options.FirstOrDefault(x => x.Option_Id == itemDirect.Option_Id)?.MediaUrl.Id_Media
                            }
                            );
                        }
                        else if (itemDirect != null && itemDirect.Stock < int.Parse(listDirectItem[2]))
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus3");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                            return;
                        }
                        else
                        {
                            await pdjService.ButtonCLick("modalPaymentStatus4");
                            await Task.Delay(5000);
                            await pdjService.GoBack();
                            return;
                        }

                    }

                    hitungTotal();

                    var refresh = NewPdjService.RefreshDataMainKonten();
                    var refreshTask = NewPdjService.RefreshDataAuthKonten();
                    await Task.WhenAll(refreshTask, refresh);

                    var data = GlobalDataService?.GlobalData?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
                    var dataZ = GlobalDataService?.GlobalData?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);

                    if(data != null)
                    {
                        data.selectShipping = false;
                    }

                    if(dataZ != null)
                    {
                        dataZ.selectPayment = false;
                    }

                    StateHasChanged();

                    Pesanan_Info.Data.Header[0].Nama_Penerima = ScopedDataServices.ScopedData.DataLogin.DataUser.UserName;
                    Pesanan_Info.Data.Header[0].Telpon_Penerima = ScopedDataServices.ScopedData.DataLogin.DataUser.Telepon;
                    Pesanan_Info.Data.Header[0].Alamat_Kirim = ScopedDataServices.ScopedData.DataLogin.DataUser.Alamat;

                    loadContent = false;
                    StateHasChanged();
                
                }
                catch {
                    contentCrash = false;
                }

                await JSRuntime.InvokeVoidAsync("countdownTimer.start");
            }
        }

        public void hitungTotal() 
        {
            totalDpp = dataSourch.ListDataCart.Sum(x => ((decimal)x.Harga * (decimal)x.Qty) - (((decimal)x.Harga * (decimal)x.Qty)) * ((decimal)x.Diskon1 / (decimal)100));
            subtotal = totalDpp + paymentMethodFee + shipingMethodFee;

            if(GlobalDataService.GlobalData.ListDataNilaiDefault.FirstOrDefault().Incl_Ppn)
            {
                ppnValue = ((decimal)GlobalDataService?.GlobalData?.ListDataNilaiDefault.FirstOrDefault().Ppn / ((decimal)100 + (decimal)GlobalDataService?.GlobalData?.ListDataNilaiDefault.FirstOrDefault().Ppn)) * (decimal)subtotal;
                nettoKeranjang = (subtotal / ((decimal)1 + (decimal)((decimal)GlobalDataService?.GlobalData?.ListDataNilaiDefault.FirstOrDefault().Ppn / (decimal)100))) + ppnValue;
            } 
            else
            {
                ppnValue = ((decimal)GlobalDataService?.GlobalData?.ListDataNilaiDefault.FirstOrDefault().Ppn / (decimal)100) * (decimal)subtotal;
                nettoKeranjang = subtotal + ppnValue;
            }
        }

        public async Task SelectPaymentMethod(Pembayaran payment)
        {
            var active = GlobalDataService?.GlobalData?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);
            var method = GlobalDataService?.GlobalData?.ListDataPembayaran.FirstOrDefault(x => x.Cara_Bayar == payment.Cara_Bayar);

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
            hitungTotal();
            StateHasChanged();
        }

        public async Task SelectShipingMethod(Pengiriman shipping)
        {
            var active = GlobalDataService?.GlobalData?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
            var method = GlobalDataService?.GlobalData?.ListDataPengiriman.FirstOrDefault(x => x.No_Id == shipping.No_Id);

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
            hitungTotal();
            StateHasChanged();
        }

        public async Task NavigateToDetailProduk(string id)
        {
            NavigationManager.NavigateTo($"/detail-produk/{id}");
        }

        public async Task FinisedOrder()
        {

            dataKirim = GlobalDataService?.GlobalData?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
            dataLayanan = GlobalDataService?.GlobalData?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);

            if (dataKirim == null || dataLayanan == null)
            {
                alertPayment = true;
            }
            else
            {
                alertPayment = false;
            }

            if (Pesanan_Info.Data.Header[0].Nama_Penerima == "" || Pesanan_Info.Data.Header[0].Telpon_Penerima == "" || Pesanan_Info.Data.Header[0].Alamat_Kirim == "")
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
            NavigationManager.NavigateTo($"/cart-page/{dataUrl}", false);
        }


        public async Task refreshStatus(Pesanan.Headers item)
        {
            loadProses = true;
            loadRefresh = true;

            StateHasChanged();

            var dataOrder = await NewPdjService.GetPesananApi(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_bukti = item.No_Bukti
                }
            });

            Pesanan_Create.Data.Header.Clear();
            StateHasChanged();


            foreach (var produk in dataOrder.Data.Header)
            {
                if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
                {
                    var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                    produk.QrBase64 = base64Image;
                }

                Pesanan_Create.Data.Header.Add(produk);
                StateHasChanged();
            }

            StateHasChanged();



            loadProses = false;
            loadRefresh = false;
            StateHasChanged();

            if (Pesanan_Create.Data.Header.FirstOrDefault()?.Status_Order_1 != "belum bayar")
            {
                while (countRedirect != 0)
                {
                    await Task.Delay(1000);
                    countRedirect--;
                    StateHasChanged();
                }

                await directStatus(Pesanan_Create.Data.Header.FirstOrDefault());
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
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", $"QR-PDJ-{item.No_Tracking}-{DateTime.Now.ToLocalTime()}.png", base64Data);
            }
        }
        
        public async Task PayOrder()
        {
            loadProses = true;
            loadRefresh = true;
            StateHasChanged();
            dataKirim = GlobalDataService?.GlobalData?.ListDataPengiriman.FirstOrDefault(x => x.selectShipping == true);
            dataLayanan = GlobalDataService?.GlobalData?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);

            List<object> ListDataItem = new List<object>();
            List<object> ListDataBiaya = new List<object>();


            foreach (var item in dataSourch.ListDataCart)
            {
                item.Jenis_Item = "stock";
                item.Harga_Satuan = item.Harga ?? 0;

                var data_item = new
                {
                    Jenis_Item = item.Jenis_Item,
                    Id_Variant = item.Id_Variant,
                    Qty = item.Qty,
                    Harga_Satuan = (item?.Harga - (item?.Harga * (item?.Diskon1 / 100)))
                };

                ListDataItem.Add(data_item);
            }

            List<object> Layanan = new List<object>
            {
                new
                {
                    Jenis_Item = "Ongkir",
                    Id_Variant = "Ongkir",
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
                Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                Status_Bayar = "",
                Id_Payment_Gateway = "",
                Payment_Type = dataLayanan.Cara_Bayar,
                Nama_Penerima = Pesanan_Info.Data.Header[0].Nama_Penerima,
                Telpon_Penerima = Pesanan_Info.Data.Header[0].Telpon_Penerima,
                Alamat_Kirim = Pesanan_Info.Data.Header[0].Alamat_Kirim,
                Id_Shipping_Option = dataKirim.No_Id,
                Items = ListDataItem,
                Biaya = Layanan,
                ambil_dari_cart = CartList == "cart" ? true : false
            };

            await JSRuntime.InvokeVoidAsync("countdownTimer.pause");

            var dataCard = await NewPdjService.CudPesananApi("TAMBAH", dataOrderCart, new ParameterApi
            {
                routeApi = "order",
                prosesApi = "create",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl()
            });


            var data = dataCard.messageResponse.Split("%section%");
            dataSourch.DataResponsePesanan = dataCard.dataRespons;

            if (dataSourch.DataResponsePesanan.BuatPesanan.StatusCode == 200 && dataSourch.DataResponsePesanan.Payment.StatusCode == 200)
            {
                var noBukti = data.Count() > 1 ? data[1] : "";

                var dataOrder = await NewPdjService.GetPesananApi(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    { 
                        no_bukti = dataSourch.DataResponsePesanan.BuatPesanan.NewId
                    }
                });

                Pesanan_Create.Data.Header.Clear();
                StateHasChanged();

                foreach (var produk in dataOrder.Data.Header)
                {
                    if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
                    {
                        var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                        produk.QrBase64 = base64Image;
                    }

                    Pesanan_Create.Data.Header.Add(produk);
                    StateHasChanged();
                }


                loadProses = false;
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("buttonClick", "#modalPaymentStatus2");

                if (Pesanan_Create.Data.Header.FirstOrDefault() == null) { 
                    loadRefresh = true;
                    Pesanan_Create.Data.Header.Add(new Headers { No_Bukti = dataSourch.DataResponsePesanan.BuatPesanan.NewId });
                } 
                else {
                    loadRefresh = false;
                }

            }
            else if(dataSourch.DataResponsePesanan.BuatPesanan.StatusCode == 200 && dataSourch.DataResponsePesanan.Payment.StatusCode != 200)
            {
                StateHasChanged();
                await pdjService.ButtonCLick("modalPaymentStatus5");
                await Task.Delay(3000);
                NavigationManager.NavigateTo("/cart-page/belum-bayar", false);
                return;
            }
            else
            {
                alertCreate = true;
                loadProses = false; 
                StateHasChanged();
                return;
            }

        }
    }
}
