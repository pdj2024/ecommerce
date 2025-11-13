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
using System.Reflection;
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

        [Parameter] public int Quantity { get; set; } = 1;
        public decimal? nettoKeranjang { get; set; } = 0;
        public decimal? nettoKeranjangChange { get; set; } = 0;
        public decimal? totalDpp { get; set; } = 0;
        public decimal? shipingMethodFee { get; set; } = 0;
        public decimal? paymentMethodFee { get; set; } = 0;
        public decimal? ppnValue { get; set; } = 0;
        public decimal? subTotal { get; set; } = 0;
        [Parameter] public string pageTabs { get; set; } = "";
        [Parameter] public EventCallback<int> QuantityChanged { get; set; }

        public bool loadProsesNetto { get; set; } = false;

        private System.Timers.Timer? _debounceTimer;

        public bool[] PesananLoad { get; set; } = [false, false, false, false];

        public int infoSubmit { get; set; } = 0;
        public bool readOnlyProfile { get; set; } = true;
        public bool updateProfil { get; set; } = false;
        public bool readOnlyAkun { get; set; } = true;
        public bool infoChange { get; set; } = false;
        public bool checkAll { get; set; } = true;
        public bool loadProses { get; set; } = false;
        public bool? infoPembatalan { get; set; } = null;
        public bool loadRefresh { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public Pesanan Pesanan_Create { get; set; } = new Pesanan();
        public Pesanan.Headers Pesanan_Change { get; set; } = new();

        MyHeadProfile header_page;
        MyFooter footer_page;

        public SourchData dataSourch { get; set; } = new();

        public int dataQty { get; set; } = 0;
        public int dataChange { get; set; } = 0;





        public async void changeLogin(string value, bool direct = false, string status = "")
        {
            if (value == "otp-view")
            {

                if ((ModelRegist.NoTlp == "" || ModelRegist.NoTlp == null) && status == "nomer")
                {
                    infoSubmit = 1;
                    StateHasChanged();
                    return;
                }

                else if ((ModelRegist.Password == "" || ModelRegist.Password == null) && status == "password")
                {
                    infoSubmit = 1;
                    StateHasChanged();
                    return;
                }

                else if (status == "nomer")
                {
                    var Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlagChange");

                    if (Numberinfo == "FALSE")
                    {
                        infoSubmit = 2;
                        StateHasChanged();
                        return;
                    }
                    else
                    {
                        var statusOtp = await JSRuntime.InvokeAsync<bool>("sendOTP", Numberinfo);

                        if (statusOtp)
                        {
                            await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);
                            dataChange = 1;
                            StateHasChanged();
                            return;
                        }
                        else
                        {
                            infoSubmit = 2;
                            StateHasChanged();
                            return;
                        }
                    }
                }

                else if(status == "password")
                {
                    var statusOtp = await JSRuntime.InvokeAsync<bool>("sendOTP", ScopedDataServices?.ScopedData?.DataLogin?.DataUser?.Telepon);

                    if (statusOtp)
                    {

                        await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);
                        dataChange = 2;
                        StateHasChanged();
                        return;
                    }
                    else
                    {
                        infoSubmit = 2;
                        StateHasChanged();
                        return;
                    }
                } 
                
                else
                {
                    StateHasChanged();
                    return;
                }

            }
            else
            {
                infoSubmit = 0;
                ModelRegist.NoTlp = "";
                ModelRegist.Password = "";

                await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);

            }
        }

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
                NavigationManager.NavigateTo("/", true);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var RefreshDataMainKonten = NewPdjService.RefreshDataMainKonten();

                var metodeBayarTask = NewPdjService.GetApi<Pembayaran>(new ParameterApi
                {
                    routeApi = "payment",
                    prosesApi = "option",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                        orderDir = "asc",
                    }
                });
                
                if (pageTabs == "belum-bayar") 
                {
                    dataSourch.DataPesananBelum = await NewPdjService.showMorePesanan(new ParameterApi
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
                } 
                
                else if (pageTabs == "dikemas") 
                {
                    dataSourch.DataPesananDikemas = await NewPdjService.showMorePesanan(new ParameterApi
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
                } 
                
                else if (pageTabs == "dikirim") 
                {
                    dataSourch.DataPesananDikirim = await NewPdjService.showMorePesanan(new ParameterApi
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
                } 
                
                else if (pageTabs == "histori") 
                {
                    dataSourch.DataPesananHistori = await NewPdjService.showMorePesanan(new ParameterApi
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
                }
                
                else if (pageTabs == "keranjang")
                {

                }
                
                else if (pageTabs == "setting")
                {
                    await JSRuntime.InvokeVoidAsync("initButtonFlagChange");
                }
                else
                {
                    NavigationManager.NavigateTo("/", true);
                }


                // Tunggu semua task selesai
                await Task.WhenAll(RefreshDataMainKonten, metodeBayarTask);

                // Assign semua data sekaligus
                dataSourch.ListDataPembayaran.Clear();
                dataSourch.ListDataPembayaran.AddRange(GlobalDataService.GlobalData.ListDataPembayaran);

                ScopedDataServices.ScopedData.Finised = true;
                StateHasChanged();

                loadContent = false;
                await SelectItemCart(null);
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

        public async Task editProfile(bool data, string ket)
        {
            if (ket == "password")
            {
            }
            else if (ket == "number")
            {
            }
            else if (ket == "cancel")
            {
                var infoCustomer = await NewPdjService.GetApi<DataUser>(new ParameterApi
                {
                    routeApi = "customer",
                    prosesApi = "getAll",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    }
                });

                ScopedDataServices.ScopedData.DataLogin.DataUser = infoCustomer?.Data?.FirstOrDefault() ?? new();
                ScopedDataServices.ScopedData.DataLogin.DataUser.Password = "xample_pass";

                var dataUser = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == ScopedDataServices?.ScopedData?.DataLogin?.No_Id);
                dataUser.DataUser = infoCustomer?.Data?.FirstOrDefault() ?? new();
                dataUser.DataUser.Password = "xample_pass";

                readOnlyProfile = !data;
                updateProfil = false;
            }
            else if (ket == "save-profil")
            {
                object itemProfile = new
                {
                    userName = ScopedDataServices.ScopedData.DataLogin.DataUser.UserName,
                    email = ScopedDataServices.ScopedData.DataLogin.DataUser.Email,
                    kota = ScopedDataServices.ScopedData.DataLogin.DataUser.Kota,
                    alamat = ScopedDataServices.ScopedData.DataLogin.DataUser.Alamat,
                    status = ScopedDataServices.ScopedData.DataLogin.DataUser.Status
                };

                var dataUpdate = await NewPdjService.UpdateApi<DataUser>(new ParameterApi
                {
                    routeApi = "customer",
                    prosesApi = "update",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_id = ScopedDataServices.ScopedData.DataLogin.No_Id
                    }
                }, itemProfile);

                await pdjService.ShowConsoleData(itemProfile);

                if (dataUpdate?.StatusCode == 200)
                {
                    readOnlyProfile = !data;
                    updateProfil = false;
                }
                else
                {
                    updateProfil = true;
                }
            }
            else if (ket == "profil")
            {
                readOnlyProfile = !data;
            }

            StateHasChanged();
        }

        public async Task ChangePaymentMethod(Pesanan.Headers item)
        {
            loadProses = true;
            StateHasChanged();

            var active = dataSourch?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);
            var dataHeader = dataSourch.DataPesananBelum.Header.FirstOrDefault(x => x.No_Bukti == item.No_Bukti);

            object data = new { no_bukti = item.No_Bukti, payment_type = active.Cara_Bayar };

            var change = await NewPdjService.ChangePayment(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "change-payment",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl()
            }, data);

            if (change != null)
            {
                await refreshAllPesanan();
                StateHasChanged();

                await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModalChange");
                await JSRuntime.InvokeVoidAsync("buttonClick", $".plus-btn-accord-{@item.No_Bukti}");

            } 
            else
            {
                infoChange = true;
            }

            loadProses = false;
            StateHasChanged();

        }

        private void checkAllItem(ChangeEventArgs e)
        {
            checkAll = Convert.ToBoolean(e.Value);
            if (checkAll)
            {
                nettoKeranjang = 0;
                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (!dataCart.SelectCart)
                    {
                        dataCart.SelectCart = true;
                    }

                    nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                }
            }
            else
            {
                nettoKeranjang = 0;
                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (dataCart.SelectCart)
                    {
                        dataCart.SelectCart = false;
                    }
                }
            }
        }

        public async Task refreshStatus(Pesanan.Headers item)
        {
            loadProses = true;
            loadRefresh = true;
            StateHasChanged();
            dataSourch.DataPesanan = await NewPdjService.GetPesananApi(new ParameterApi
            {
                routeApi = "order",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    no_bukti = item.No_Bukti,
                    show_media = "true",
                    orderBy = "no_bukti",
                    orderDir = "desc",
                }
            });


            var produk = dataSourch?.DataPesanan.Header.FirstOrDefault(x => x.No_Bukti == item?.No_Bukti);

            if (produk != null && produk?.Metode_Bayar == "QRIS" && produk?.Status_Order_1 == "belum bayar")
            {
                var base64Image = await GenerateImage(produk?.Payment_Number_1, produk?.Expiry_Time);
                produk.QrBase64 = base64Image;
            }

            Pesanan_Create.Header.Clear();
            StateHasChanged();

            Pesanan_Create.Header.Add(produk);

            if (produk?.Status_Order_1 != item?.Status_Order_1)
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", $"#pills-{produk?.Status_Order_1}-tab");
                await refreshAllPesanan();
            }



            loadProses = false;
            loadRefresh = false;
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
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", $"QR-PDJ-{item.No_Tracking}-{DateTime.Now.ToLocalTime()}.png", base64Data);
            }
        }

        private async Task GantiMethod(Pesanan.Headers headItem)
        {
            Pesanan_Change = headItem;
            await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModal");
            StateHasChanged();

            foreach(var listBayar in dataSourch.ListDataPembayaran)
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

            totalDpp  = dataSourch?.DataPesananBelum?.Items.Where(x => x.No_Bukti == headItem.No_Bukti).Sum(x => x.Harga_Satuan * x.Qty);
            shipingMethodFee  = dataSourch?.DataPesananBelum?.Biaya.FirstOrDefault(x => x.No_Bukti == headItem.No_Bukti && x.Jenis_Item.ToLower() == "layanan")?.Harga_Satuan;
            paymentMethodFee  = dataSourch?.DataPesananBelum?.Biaya.FirstOrDefault(x => x.No_Bukti == headItem.No_Bukti && x.Jenis_Item.ToLower() == "ongkir")?.Harga_Satuan;

            hitungTotal();
            await JSRuntime.InvokeVoidAsync("buttonClick", $"#buttonChangeMethod");
            StateHasChanged();
            
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
            await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModal");
            StateHasChanged();

            
            await JSRuntime.InvokeVoidAsync("buttonClick", $"#buttonBatalMethod");
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

        public async Task clickCopy(Pesanan.Headers item, string init = "")
        {
            await JSRuntime.InvokeVoidAsync("copyVa", init);
        }

        public async void openDropdown(string id_kode)
        {
            await JSRuntime.InvokeVoidAsync("initItemsOrder", id_kode);
        }

        public async Task openPayment(Pesanan.Headers produk)
        {
            Pesanan_Create.Header.Clear();
            loadRefresh = true;
            StateHasChanged();

            if (produk.Metode_Bayar == "QRIS" && produk.Status_Order_1 == "belum bayar")
            {
                var base64Image = await GenerateImage(produk.Payment_Number_1, produk.Expiry_Time);
                produk.QrBase64 = base64Image;
            }

            Pesanan_Create.Header.Add(produk);

            if (Pesanan_Create.Header.Count() >= 2) {
                var data = Pesanan_Create.Header.LastOrDefault();
                Pesanan_Create.Header.Remove(data);
            }

            loadRefresh = false;
            StateHasChanged();
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


            shipingMethodFee = method.Fee;
            hitungTotal();
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
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_id = cart.Id_Cart
                }
            };
            loadProses = true;
            StateHasChanged();
            (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("HAPUS", cart, paramApi);

            if (statusRespon && statusCode == 200)
            {
                await RefreshCart();
            }

            loadProses = false;
            StateHasChanged();
        }

        private async Task Increase(string id_cart)
        {
            var data = ScopedDataServices?.ScopedData?.ListDataCart?.FirstOrDefault(x => x.Id_Cart == id_cart);

            if (data.Qty >= data.Stock)
            {
                data.Show_Min_Order = false;
                data.Show_Max_Order = true;
                return;
            }

            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }

            data.Show_Min_Order = false;
            data.Show_Max_Order = false;

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl()
            };

            dataQty++;
            data.Qty++;

            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            _debounceTimer = new System.Timers.Timer(500); // 500ms delay
            _debounceTimer.Elapsed += async (s, e) =>
            {
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();

                await InvokeAsync(async () =>
                {

                    Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = dataQty, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id };
                    (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("TAMBAH", itemcart, paramApi);


                    if (statusRespon && statusCode == 200)
                    {
                        await RefreshCart(id_cart);
                    }


                    nettoKeranjang = 0;
                    foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                    {
                        if (!dataCart.SelectCart) continue;
                        nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                    }

                    dataQty = 0;
                });
            };

            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();


            loadProsesNetto = false;
            StateHasChanged();
        }

        private async Task Decrease(string id_cart)
        {

            var data = ScopedDataServices?.ScopedData?.ListDataCart?.FirstOrDefault(x => x.Id_Cart == id_cart);

            if (data.Qty <= data.Min_Order)
            {
                data.Show_Min_Order = true;
                data.Show_Max_Order = false;
                return;
            }

            if (data.SelectCart)
            {
                loadProsesNetto = true;
                StateHasChanged();
            }

            data.Show_Min_Order = false;
            data.Show_Max_Order = false;

            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "create",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl()
            };


            if (data.Qty > 1)
            {
                dataQty--;
                data.Qty--;


                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();

                _debounceTimer = new System.Timers.Timer(500); // 500ms delay
                _debounceTimer.Elapsed += async (s, e) =>
                {
                    _debounceTimer?.Stop();
                    _debounceTimer?.Dispose();

                    await InvokeAsync(async () =>
                    {

                        Cart itemcart = new Cart { Id_Variant = data.Id_Variant, Qty = dataQty, Id_Customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id };
                        (string statusMessage, bool statusRespon, int statusCode, string proses) = await NewPdjService.CudCartApi("TAMBAH", itemcart, paramApi);


                        if (statusRespon && statusCode == 200)
                        {
                            await RefreshCart(id_cart);
                        }


                        nettoKeranjang = 0;
                        foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                        {
                            if (!dataCart.SelectCart) continue;
                            nettoKeranjang += (decimal)dataCart.Qty * ((decimal)dataCart?.Harga - ((decimal)dataCart?.Harga * ((decimal)dataCart?.Diskon1 / 100)));
                        }

                        dataQty = 0;
                    });
                };

                _debounceTimer.AutoReset = false;
                _debounceTimer.Start();
            }

            loadProsesNetto = false;
            StateHasChanged();
        }

        public async Task SelectItemCart(Cart cart)
        {
            loadProsesNetto = true;
            StateHasChanged();

            if (cart != null)
            {
                var data = ScopedDataServices?.ScopedData?.ListDataCart?.FirstOrDefault(x => x.Id_Cart == cart.Id_Cart);
                data.SelectCart = !data.SelectCart;

                nettoKeranjang = 0;
                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * (dataCart.Harga - (dataCart.Harga * (dataCart.Diskon1 / 100)));
                }
            }
            else
            {
                nettoKeranjang = 0;
                foreach (var dataCart in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (!dataCart.SelectCart) continue;
                    nettoKeranjang += dataCart.Qty * (dataCart.Harga - (dataCart.Harga * (dataCart.Diskon1 / 100)));
                }
            }


            loadProsesNetto = false;
            StateHasChanged();
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

            var DataPesananHistori = NewPdjService.showMorePesanan(new ParameterApi
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

            var dataSummary = NewPdjService.RefreshDataMainKonten();

            await Task.WhenAll(DataPesananBelum, DataPesananDikemas, DataPesananDikirim, DataPesananHistori, dataSummary);



            dataSourch.DataPesananBelum = DataPesananBelum.Result;
            dataSourch.DataPesananDikemas = DataPesananDikemas.Result;
            dataSourch.DataPesananDikirim = DataPesananDikirim.Result;
            dataSourch.DataPesananHistori = DataPesananHistori.Result;
        }

        public async Task RefreshCart(string data_id = null)
        {
            ParameterApi paramApi = new ParameterApi
            {
                routeApi = "cart",
                prosesApi = "getAll",
                token = ScopedDataServices.ScopedData.DataLogin.token,
                refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                    show_media = "true",
                    orderBy = "Updated_At",
                    orderDir = "desc"
                }
            };
            ListDataCartCheck.Clear();
            ListDataCartCheck.AddRange(ScopedDataServices?.ScopedData?.ListDataCart?.FindAll(x => x.SelectCart == true));
            var DataCartList = await NewPdjService.GetApi<Cart>(paramApi);

            if (data_id == null)
            {
                ScopedDataServices.ScopedData.ListDataCart = DataCartList.Data;
            }
            else
            {
                var DataShow = ScopedDataServices?.ScopedData?.ListDataCart.FirstOrDefault(x => x.Id_Cart == data_id);
                DataShow.Qty = DataCartList.Data.FirstOrDefault(x => x.Id_Cart == data_id).Qty;
            }

            if (ListDataCartCheck.Count() >= 1)
            {
                foreach (var item in ScopedDataServices?.ScopedData?.ListDataCart)
                {
                    if (ListDataCartCheck.Any(x => x.Id_Cart == item.Id_Cart))
                    {
                        item.SelectCart = true;
                    }
                }
            }

            await SelectItemCart(null);
            StateHasChanged();
        }

        public void RefreshLoad() 
        {
            StateHasChanged();
        }

        public async Task CheckoutCart()
        {

            string text = "";
            foreach (var item in ScopedDataServices?.ScopedData?.ListDataCart)
            {
                if (item.SelectCart) text += $"{item.Id_Cart}%section%";
            }
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            NavigationManager.NavigateTo($"/payment-page/cart/{encoded}", forceLoad: true);
        }

        private async Task showMore(string pageTipe = "")
        {
            loadProses = true;
            string lastCursor = "";
            StateHasChanged();

            if (pageTipe == "belum bayar")
            {

                lastCursor = dataSourch.DataPesananBelum.LastCursor;
                var data = await NewPdjService.showMorePesanan(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                        show_media = "true",
                        orderDir = "desc",
                        status_order_1 = pageTipe,
                        status = "aktif",
                        orderBy = "no_bukti",
                        cursor_no_bukti = lastCursor,
                        pagesize = 5,
                    }
                });

                dataSourch.DataPesananBelum.Header.AddRange(data.Header);
                dataSourch.DataPesananBelum.Items.AddRange(data.Items);
                dataSourch.DataPesananBelum.Biaya.AddRange(data.Biaya);
                dataSourch.DataPesananBelum.hasMore = data.hasMore;
                dataSourch.DataPesananBelum.TotalCount = data.TotalCount;
                dataSourch.DataPesananBelum.LastCursor = data.LastCursor;

            } 

            else if (pageTipe == "dikemas")
            {
                lastCursor = dataSourch.DataPesananDikemas.LastCursor;
                var data = await NewPdjService.showMorePesanan(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                        show_media = "true",
                        orderDir = "desc",
                        status_order_1 = pageTipe,
                        status = "aktif",
                        orderBy = "no_bukti",
                        cursor_no_bukti = lastCursor,
                        pagesize = 5,
                    }
                });
                
                dataSourch.DataPesananDikemas.Header.AddRange(data.Header);
                dataSourch.DataPesananDikemas.Items.AddRange(data.Items);
                dataSourch.DataPesananDikemas.Biaya.AddRange(data.Biaya);
                dataSourch.DataPesananDikemas.hasMore = data.hasMore;
                dataSourch.DataPesananDikemas.TotalCount = data.TotalCount;
                dataSourch.DataPesananDikemas.LastCursor = data.LastCursor;
            } 

            else if (pageTipe == "dikirim")
            {
                lastCursor = dataSourch.DataPesananDikirim.LastCursor;
                var data = await NewPdjService.showMorePesanan(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                        show_media = "true",
                        orderDir = "desc",
                        status_order_1 = pageTipe,
                        status = "aktif",
                        orderBy = "no_bukti",
                        cursor_no_bukti = lastCursor,
                        pagesize = 5,
                    }
                });

                dataSourch.DataPesananDikirim.Header.AddRange(data.Header);
                dataSourch.DataPesananDikirim.Items.AddRange(data.Items);
                dataSourch.DataPesananDikirim.Biaya.AddRange(data.Biaya);
                dataSourch.DataPesananDikirim.hasMore = data.hasMore;
                dataSourch.DataPesananDikirim.TotalCount = data.TotalCount;
                dataSourch.DataPesananDikirim.LastCursor = data.LastCursor;
            } 

            else if (pageTipe == "selesai")
            {
                lastCursor = dataSourch.DataPesananHistori.LastCursor;
                var data = await NewPdjService.showMorePesanan(new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                        show_media = "true",
                        orderDir = "desc",
                        status_order_1 = pageTipe,
                        status = "aktif",
                        orderBy = "no_bukti",
                        cursor_no_bukti = lastCursor,
                        pagesize = 5,
                    }
                });

                dataSourch.DataPesananHistori.Header.AddRange(data.Header);
                dataSourch.DataPesananHistori.Items.AddRange(data.Items);
                dataSourch.DataPesananHistori.Biaya.AddRange(data.Biaya);
                dataSourch.DataPesananHistori.hasMore = data.hasMore;
                dataSourch.DataPesananHistori.TotalCount = data.TotalCount;
                dataSourch.DataPesananHistori.LastCursor = data.LastCursor;
            } 

            
            loadProses = false;
            StateHasChanged();
        }
    }
}
