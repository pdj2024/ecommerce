using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor.Rendering;
using Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using static Models.Pesanan;
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
        public bool? infoSelesaikan { get; set; } = null;
        public bool loadRefresh { get; set; } = false;
        public bool loadContent { get; set; } = true;
        public string Numberinfo { get; set; } = "";
        public Pesanan Pesanan_Create { get; set; } = new Pesanan();
        public Pesanan PesananList { get; set; } = new();
        public Pesanan.Headers Pesanan_Change { get; set; } = new();

        MyHeader header_page;
        MyFooter footer_page;

        public SourchData dataSourch { get; set; } = new();

        public int dataQty { get; set; } = 0;
        public int dataChange { get; set; } = 0;

        public async Task moveTabs(string tab, Pesanan.Headers item = null)
        {
            loadContent = tab == "setting";

            if (tab == "details")
            {
                if (item == null) return;

                NavigationManager.NavigateTo($"/cart-page/{tab}/{item.No_Tracking}", forceLoad: false);
            } 
            else
            {
                NavigationManager.NavigateTo($"/cart-page/{tab}", forceLoad: false);
            }

        }

        public async Task NumberOnlyInput(ChangeEventArgs e, string info)
        {
            var value = e.Value?.ToString() ?? "";

            bool hasPlus = value.StartsWith("+");

            value = Regex.Replace(value, @"[^0-9]", "");

            if (hasPlus)
            {
                value = "+" + value;
            }

            if (info == "phone-login")
            {

            }
            else if (info == "phone")
            {
                ModelRegist.NoTlp = value;
            }
            else if (info == "phone-change")
            {
                ModelRegist.NoTlp = value;
            }

            await JSRuntime.InvokeVoidAsync("setInputValue", info, value);
        }

        public async void changeLogin(string value, bool direct = false, string status = "")
        {
            loadLogin = true;
            StateHasChanged();

            if (value == "otp-view")
            {

                if ((ModelRegist.NoTlp == "" || ModelRegist.NoTlp == null) && status == "Nomor")
                {
                    infoSubmit = 1;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }

                else if ((ModelRegist.Password == "" || ModelRegist.Password == null) && status == "password")
                {
                    infoSubmit = 1;
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }

                if (status == "Nomor")
                {
                    Numberinfo = "";
                    Numberinfo = await JSRuntime.InvokeAsync<string>("initKlikFlagChange");

                    if (Numberinfo == "FALSE")
                    {
                        infoSubmit = 2;
                        loadLogin = false;
                        StateHasChanged();
                        return;
                    }
                    else
                    {

                        var dataCek = await NewPdjService.CheckUserRegApi(new ParameterApi
                        {
                            routeApi = "auth",
                            prosesApi = "check",
                            ParamUrl = new ParameterApi.ParameterUrl()
                        }, new LoginViewModel { NoTlp = Numberinfo });


                        if (!dataCek)
                        {
                            infoSubmit = 3;
                            loadLogin = false;
                            StateHasChanged();
                            return;
                        }


                        var statusOtp = await JSRuntime.InvokeAsync<bool>("sendOTP", Numberinfo);

                        if (statusOtp)
                        {
                            await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);
                            dataChange = 1;
                            loadLogin = false;
                            StateHasChanged();
                            return;
                        }
                        else
                        {
                            infoSubmit = 2;
                            loadLogin = false;
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
                        loadLogin = false;
                        StateHasChanged();
                        return;
                    }
                    else
                    {
                        infoSubmit = 2;
                        loadLogin = false;
                        StateHasChanged();
                        return;
                    }
                } 
                
                else
                {
                    loadLogin = false;
                    StateHasChanged();
                    return;
                }

            }
            else
            {
                infoSubmit = 0;
                ModelRegist.NoTlp = "";
                ModelRegist.Password = "";

                loadLogin = false;
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("initLoginEffect", value, direct);

            }

            loadLogin = false;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            ti = CultureInfo.CurrentCulture.TextInfo;

            Pesanan_Create.Data.Header.Add(new Pesanan.Headers());
            Pesanan_Create.Data.Items.Add(new Pesanan.Item());
            Pesanan_Create.Data.Biaya.Add(new Pesanan.Biayas());

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

            var RefreshDataMainKonten = NewPdjService.RefreshDataMainKonten();
            var RefreshDataOuth = NewPdjService.RefreshDataAuthKonten();

            await Task.WhenAll(RefreshDataMainKonten, RefreshDataOuth);

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                JSRuntime.InvokeVoidAsync("removeBackdrops");
                JSRuntime.InvokeVoidAsync("goTopInstant");

                ScopedDataServices.ScopedData.ListDataPembayaran = new();

                if (pageTabs == "belum-bayar")
                {
                    ScopedDataServices.ScopedData.DataPesananBelum = await NewPdjService.showMorePesanan(new ParameterApi
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

                    PesananList = ScopedDataServices.ScopedData.DataPesananBelum;
                }
                else if (pageTabs == "dikemas")
                {
                    ScopedDataServices.ScopedData.DataPesananDikemas = await NewPdjService.showMorePesanan(new ParameterApi
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

                    PesananList = ScopedDataServices.ScopedData.DataPesananDikemas;
                }
                else if (pageTabs == "dikirim")
                {
                    ScopedDataServices.ScopedData.DataPesananDikirim = await NewPdjService.showMorePesanan(new ParameterApi
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

                    PesananList = ScopedDataServices.ScopedData.DataPesananDikirim;
                }
                else if (pageTabs == "selesai")
                {
                    ScopedDataServices.ScopedData.DataPesananSelesai = await NewPdjService.showMorePesanan(new ParameterApi
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

                    PesananList = ScopedDataServices.ScopedData.DataPesananSelesai;
                }
                else if (pageTabs == "batal")
                {
                    ScopedDataServices.ScopedData.DataPesananBatal = await NewPdjService.showMorePesanan(new ParameterApi
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
                            status = "batal",
                            orderBy = "no_bukti",
                            pagesize = 5,
                        }
                    });

                    PesananList = ScopedDataServices.ScopedData.DataPesananBatal;
                }
                else if (pageTabs == "keranjang") { }
                else if (pageTabs == "setting") { }
                else if (pageTabs == "details")
                {

                }
                else
                {
                    NavigationManager.NavigateTo("/", false);
                }

                if (pageTabs == "setting")
                {
                    await JSRuntime.InvokeVoidAsync("initButtonFlagChange");
                }
                
                loadContent = false;
                StateHasChanged();

                ScopedDataServices.ScopedData.ListDataPembayaran.AddRange(GlobalDataService.GlobalData.ListDataPembayaran);

                StateHasChanged();
                await header_page.SelectItemCart(null);
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

        public async Task pesananSelesai(Pesanan.Headers headItem, bool direct = false)
        {
            infoSelesaikan = null;
            StateHasChanged();

            if (direct)
            {
                if (Pesanan_Change?.No_Bukti == null) return;

                object data = new { statusItem = "selesai" };

                loadProses = true;
                var PembatalanStatus = await NewPdjService.CudPesananApi("UPDATE", data, new ParameterApi
                {
                    routeApi = "order",
                    prosesApi = "updateStatus",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        no_bukti = Pesanan_Change?.No_Bukti ?? "",
                        all_item = true
                    }
                });

                if (PembatalanStatus.statusResponse)
                {
                    infoSelesaikan = true;


                    ScopedDataServices.ScopedData.DataPesananDetail = await NewPdjService.showMorePesanan(new ParameterApi
                    {
                        routeApi = "order",
                        prosesApi = "getAll",
                        token = ScopedDataServices.ScopedData.DataLogin.token,
                        refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                        ParamUrl = new ParameterApi.ParameterUrl
                        {
                            id_customer = ScopedDataServices?.ScopedData?.DataLogin?.No_Id,
                            no_bukti = Pesanan_Change?.No_Bukti,
                            show_media = "true",
                            orderDir = "desc",
                            orderBy = "no_bukti",
                            pagesize = 5,
                        }
                    });

                    await refreshAllPesanan();

                }
                else
                {
                    infoSelesaikan = false;
                }

                loadProses = false;
                StateHasChanged();
                return;
            }


            Pesanan_Change = headItem;
            StateHasChanged();
        }

        public async Task onPesananSelesai(Pesanan.Headers headItem)
        {
            infoSelesaikan = null;
            StateHasChanged();
            headItem.variabel_map_1 = true;

            Pesanan_Change = headItem;
            StateHasChanged();
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

            var active = ScopedDataServices.ScopedData?.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);
            var dataHeader = ScopedDataServices.ScopedData.DataPesananBelum.Data.Header.FirstOrDefault(x => x.No_Bukti == item.No_Bukti);

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

            var data = await NewPdjService.GetPesananApi(new ParameterApi
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


            var produk = data.Data.Header.FirstOrDefault(x => x.No_Bukti == item?.No_Bukti);

            if (produk != null && produk?.Metode_Bayar == "QRIS" && produk?.Status_Order_1 == "belum bayar")
            {
                var base64Image = await GenerateImage(produk?.Payment_Number_1, produk?.Expiry_Time);
                produk.QrBase64 = base64Image;
            }

            Pesanan_Create = new();
            StateHasChanged();

            Pesanan_Create = data;

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
            try
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModal");
            }
            catch
            {

            }
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

            try 
            {
                await JSRuntime.InvokeVoidAsync("buttonClick", $"#buttonChangeMethod");
            }
            catch 
            { 

            }
            
        }

        private async Task BatalkanPesanan (Pesanan.Headers headItem)
        {
            infoPembatalan = null;
            StateHasChanged();

            //if (direct)
            //{
            //    loadProses = true;
            //    var PembatalanStatus = await NewPdjService.CudPesananApi("TAMBAH", null , new ParameterApi
            //    {
            //        routeApi = "order",
            //        token = ScopedDataServices.ScopedData.DataLogin.token,
            //        refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
            //        prosesApi = $"batal/{Pesanan_Change.No_Bukti}",
            //        ParamUrl = new ParameterApi.ParameterUrl()
            //    });

            //    if (PembatalanStatus.statusResponse)
            //    {
            //        infoPembatalan = true;

            //        await refreshAllPesanan();


            //    } else
            //    {
            //        infoPembatalan = false;
            //    }

            //    loadProses = false;
            //    return;
            //}


            Pesanan_Change = headItem;
            //await JSRuntime.InvokeVoidAsync("buttonClick", $"#closeModal");
            //StateHasChanged();

            
            //await JSRuntime.InvokeVoidAsync("buttonClick", $"#buttonBatalMethod");
            StateHasChanged();
        }

        private async Task AuthBatalkanPesanan(Pesanan.Headers headItem, bool direct = false)
        {
            infoPembatalan = null;
            StateHasChanged();

            if (direct)
            {
                loadProses = true;
                var PembatalanStatus = await NewPdjService.CudPesananApi("TAMBAH", null, new ParameterApi
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


                }
                else
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

        public async Task clickCopy(Pesanan.Headers item, string init = "")
        {
            await JSRuntime.InvokeVoidAsync("copyVa", init);
        }

        public async void openDropdown(string id_kode)
        {
            await JSRuntime.InvokeVoidAsync("initItemsOrder", id_kode);
        }

        public async void NavigateToDetailProduk(string id)
        {
            List<string> stringData = await pdjService.DecodeEncode(true, new List<string> { id });
            NavigationManager.NavigateTo($"/detail-produk/{stringData.FirstOrDefault()}", forceLoad: true);
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

        public async Task openPaymentSection(Pesanan.Headers produk)
        {
            List<Pesanan.Biayas> biaya = PesananList?.Data.Biaya.Where(x => x.No_Bukti == produk.No_Bukti).ToList();

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

        public async Task SelectPaymentMethod(Pembayaran payment)
        {
            var active = ScopedDataServices.ScopedData.ListDataPembayaran.FirstOrDefault(x => x.selectPayment == true);
            var method = ScopedDataServices.ScopedData.ListDataPembayaran.FirstOrDefault(x => x.Cara_Bayar == payment.Cara_Bayar);

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

                lastCursor = ScopedDataServices.ScopedData.DataPesananBelum.LastCursor;
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

                ScopedDataServices.ScopedData.DataPesananBelum.Data.Header.AddRange(data.Data.Header);
                ScopedDataServices.ScopedData.DataPesananBelum.Data.Items.AddRange(data.Data.Items);
                ScopedDataServices.ScopedData.DataPesananBelum.Data.Biaya.AddRange(data.Data.Biaya);
                ScopedDataServices.ScopedData.DataPesananBelum.hasMore = data.hasMore;
                ScopedDataServices.ScopedData.DataPesananBelum.TotalCount = data.TotalCount;
                ScopedDataServices.ScopedData.DataPesananBelum.LastCursor = data.LastCursor;


                PesananList = ScopedDataServices.ScopedData.DataPesananBelum;
            } 

            else if (pageTipe == "dikemas")
            {
                lastCursor = ScopedDataServices.ScopedData.DataPesananDikemas.LastCursor;
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
                
                ScopedDataServices.ScopedData.DataPesananDikemas.Data.Header.AddRange(data.Data.Header);
                ScopedDataServices.ScopedData.DataPesananDikemas.Data.Items.AddRange(data.Data.Items);
                ScopedDataServices.ScopedData.DataPesananDikemas.Data.Biaya.AddRange(data.Data.Biaya);
                ScopedDataServices.ScopedData.DataPesananDikemas.hasMore = data.hasMore;
                ScopedDataServices.ScopedData.DataPesananDikemas.TotalCount = data.TotalCount;
                ScopedDataServices.ScopedData.DataPesananDikemas.LastCursor = data.LastCursor;

                PesananList = ScopedDataServices.ScopedData.DataPesananDikemas;
            } 

            else if (pageTipe == "dikirim")
            {
                lastCursor = ScopedDataServices.ScopedData.DataPesananDikirim.LastCursor;
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

                ScopedDataServices.ScopedData.DataPesananDikirim.Data.Header.AddRange(data.Data.Header);
                ScopedDataServices.ScopedData.DataPesananDikirim.Data.Items.AddRange(data.Data.Items);
                ScopedDataServices.ScopedData.DataPesananDikirim.Data.Biaya.AddRange(data.Data.Biaya);
                ScopedDataServices.ScopedData.DataPesananDikirim.hasMore = data.hasMore;
                ScopedDataServices.ScopedData.DataPesananDikirim.TotalCount = data.TotalCount;
                ScopedDataServices.ScopedData.DataPesananDikirim.LastCursor = data.LastCursor;

                PesananList = ScopedDataServices.ScopedData.DataPesananDikirim;
            } 

            else if (pageTipe == "selesai")
            {
                lastCursor = ScopedDataServices.ScopedData.DataPesananSelesai.LastCursor;
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

                ScopedDataServices.ScopedData.DataPesananSelesai.Data.Header.AddRange(data.Data.Header);
                ScopedDataServices.ScopedData.DataPesananSelesai.Data.Items.AddRange(data.Data.Items);
                ScopedDataServices.ScopedData.DataPesananSelesai.Data.Biaya.AddRange(data.Data.Biaya);
                ScopedDataServices.ScopedData.DataPesananSelesai.hasMore = data.hasMore;
                ScopedDataServices.ScopedData.DataPesananSelesai.TotalCount = data.TotalCount;
                ScopedDataServices.ScopedData.DataPesananSelesai.LastCursor = data.LastCursor;

                PesananList = ScopedDataServices.ScopedData.DataPesananSelesai;
            } 

            else if (pageTipe == "batal")
            {
                lastCursor = ScopedDataServices.ScopedData.DataPesananBatal.LastCursor;
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

                ScopedDataServices.ScopedData.DataPesananBatal.Data.Header.AddRange(data.Data.Header);
                ScopedDataServices.ScopedData.DataPesananBatal.Data.Items.AddRange(data.Data.Items);
                ScopedDataServices.ScopedData.DataPesananBatal.Data.Biaya.AddRange(data.Data.Biaya);
                ScopedDataServices.ScopedData.DataPesananBatal.hasMore = data.hasMore;
                ScopedDataServices.ScopedData.DataPesananBatal.TotalCount = data.TotalCount;
                ScopedDataServices.ScopedData.DataPesananBatal.LastCursor = data.LastCursor;

                PesananList = ScopedDataServices.ScopedData.DataPesananBatal;
            } 

            
            loadProses = false;
            StateHasChanged();
        }

        private async Task KirimPesan(Pesanan.Headers item)
        {
            var lines = new List<string>
            {
                $"*Nama*: {ScopedDataServices.ScopedData.DataLogin.DataUser.UserName}",
                $"*Pesanan: {item.No_Tracking}*",
                $"*Pesan*:",
                $"Bagaimana progres pesanan saya?"
            };

            var message = string.Join("\n", lines);
            var encoded = Uri.EscapeDataString(message);

            var target = GlobalDataService?.GlobalData?.ListDataCompany?.FirstOrDefault()?.No_Wa_1;
            var to = target.Replace("+", "");
            var url = $"https://wa.me/{to}?text={encoded}";

            await JSRuntime.InvokeVoidAsync("open", url, "_blank");
        }

    }
}
