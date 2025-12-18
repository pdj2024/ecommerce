using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Services
{
    public partial class NewPdjService
    {


        private readonly GlobalDataService GlobalDataService;
        private readonly ScopedDataServices ScopedDataServices;
        private readonly pdjService PdjService;
        private readonly IJSRuntime? JSRuntime;
        private readonly NavigationManager? NavigationManager;
        private readonly IConfiguration? _config;

        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NewPdjService(GlobalDataService globalDataService, ScopedDataServices scopedDataServices, pdjService pdjService, IJSRuntime jsRuntime, NavigationManager navigationManager, IHttpContextAccessor httpCon, IConfiguration config)
        {
            GlobalDataService = globalDataService;
            ScopedDataServices = scopedDataServices;
            PdjService = pdjService;
            JSRuntime = jsRuntime;
            NavigationManager = navigationManager;
            _httpContextAccessor = httpCon;
            _config = config;
        }

        public HttpContext? HttpContext => _httpContextAccessor.HttpContext;
        public class StatusResponse
        {
            public int StatusCode { get; set; } = 0;
            public string Message { get; set; } = "";
            public string No_Bukti { get; set; } = "";
            public string token { get; set; } = "";
            public string refreshToken { get; set; } = "";
        }


        public async Task<bool> ChangeAkunApi(ParameterApi urlParam, object data)
        {
            var config = await initConfigApi(urlParam);
            var tbListProduk = new DataUser();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var requestBody = data;

            var jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await config.httpReq.PostAsync(config.UrlApi, content, config.cancelTimer.Token);
                var checking = await CheckResponseAPI(response, config.UrlApi, $"Change-{urlParam.prosesApi}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (checking.statusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        urlParam.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(urlParam);

                        response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }

            }
            catch (Exception ex)
            {
                writeLog($"Regist error: {ex.Message}");
            }

            return false;
        }



        public async Task<(string messageResponse, bool statusResponse, int codeNumber, string proses)> CudCartApi(string proses, Cart cart = null, ParameterApi urlParam = null)
		{
			var config = await initConfigApi(urlParam);
			var result = (statusCode: 0, statusResponse: false, MessageInfo: "");
			object requestBody = null;
			string jsonContent = "";
			HttpResponseMessage response;

			var jsonSettings = new JsonSerializerSettings
			{
				Formatting = Newtonsoft.Json.Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};

			try
			{
				switch (proses)
				{
					case "HAPUS":
						response = await config.httpReq.DeleteAsync(config.UrlApi, config.cancelTimer.Token);
						break;

					case "TAMBAH":
						requestBody = new
						{
							id_Customer = cart.Id_Customer,
							id_Variant = cart.Id_Variant,
							add_qty = cart.Qty
						};

						jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
						var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

						response = await config.httpReq.PostAsync(config.UrlApi, content, config.cancelTimer.Token);
						break;

					default:
						jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
						content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

						response = await config.httpReq.PutAsync(config.UrlApi, content, config.cancelTimer.Token);
						break;
				}

				result = await CheckResponseAPI(response, config.UrlApi, proses);

				if (response.IsSuccessStatusCode)
				{

				}
				else if (result.statusCode == 401)
				{
					var statusRefresh = await RefreshToken();

					if (statusRefresh)
					{
						urlParam.token = ScopedDataServices.ScopedData.DataLogin.token;
						config = await initConfigApi(urlParam);

						response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

						result = await CheckResponseAPI(response, config.UrlApi, "BACA");

						if (response.IsSuccessStatusCode)
						{

						}
					}
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
				}

				return (result.MessageInfo, result.statusResponse, result.statusCode, proses);
			}
			catch (Exception ex)
			{
				if (ex is OperationCanceledException)
				{
					writeLog("Gagal CUD Data : Request ke API terlalu lama (timeout).");
				}
				else if (ex is HttpRequestException)
				{
					writeLog($"Gagal CUD Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
				}
				else
				{
					writeLog($"Gagal CUD Data : Terjadi error: {ex.Message}");
				}
			}

			return ("Terjadi Kesalahan: Gagal memproses data", false, 500, proses);
		}

		public async Task<(string messageResponse, bool statusResponse, int codeNumber, string proses, Pesanan.PaymentResponse dataRespons)> CudPesananApi(string proses, object order = null, ParameterApi urlParam = null)
		{
			var config = await initConfigApi(urlParam);
			var result = (statusCode: 0, statusResponse: false, MessageInfo: "");
			object requestBody = null;
			string jsonContent = "";
			HttpResponseMessage response;

			var dataResponses = new Pesanan.PaymentResponse();

			var jsonSettings = new JsonSerializerSettings
			{
				Formatting = Newtonsoft.Json.Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};

			try
			{
				switch (proses)
				{
					case "HAPUS":
						response = await config.httpReq.DeleteAsync(config.UrlApi, config.cancelTimer.Token);
						break;

					case "TAMBAH":
						requestBody = order;

						jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
						var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

						response = await config.httpReq.PostAsync(config.UrlApi, content, config.cancelTimer.Token);
						break;

					default:
                        requestBody = order;

                        jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
						content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

						response = await config.httpReq.PutAsync(config.UrlApi, content, config.cancelTimer.Token);
						break;
				}

				result = await CheckResponseAPI(response, config.UrlApi, proses);

				if (response.IsSuccessStatusCode)
				{
					var json = await response.Content.ReadAsStringAsync();
					using var jsonDoc = JsonDocument.Parse(json);
					var root = jsonDoc.RootElement;

					dataResponses = JsonConvert.DeserializeObject<Pesanan.PaymentResponse>(root.GetRawText());
				}
				else if (result.statusCode == 401)
				{
					var statusRefresh = await RefreshToken();

					if (statusRefresh)
					{
						urlParam.token = ScopedDataServices.ScopedData.DataLogin.token;
						config = await initConfigApi(urlParam);

						response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

						result = await CheckResponseAPI(response, config.UrlApi, "BACA");

						if (response.IsSuccessStatusCode)
						{
							var json = await response.Content.ReadAsStringAsync();
							using var jsonDoc = JsonDocument.Parse(json);
							var root = jsonDoc.RootElement;

							dataResponses = JsonConvert.DeserializeObject<Pesanan.PaymentResponse>(root.GetRawText());
						}
					}
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
				}


				return (result.MessageInfo, response.IsSuccessStatusCode, result.statusCode, proses, dataResponses);
			}
			catch (Exception ex)
			{
				if (ex is OperationCanceledException)
				{
					writeLog("Gagal CUD Data : Request ke API terlalu lama (timeout).");
				}
				else if (ex is HttpRequestException)
				{
					writeLog($"Gagal CUD Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
				}
				else
				{
					writeLog($"Gagal CUD Data : Terjadi error: {ex.Message}");
				}
			}

			return ("Terjadi Kesalahan: Gagal memproses data", false, 500, proses, dataResponses);
		}


		public async Task<Pesanan> showMorePesanan(ParameterApi urlParam)
		{
			var config = await initConfigApi(urlParam);
            Pesanan tbListProduk = new Pesanan();

			try
			{
				var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

				var checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                if (checking.statusCode == 401)
				{
					var statusRefresh = await RefreshToken();

					if (statusRefresh)
					{
						urlParam.token = ScopedDataServices.ScopedData.DataLogin.token;
						config = await initConfigApi(urlParam);

						response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

						checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

						if (!response.IsSuccessStatusCode)
						{
                            return tbListProduk;
                        }
					}
					else
					{
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
				}

                var json = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                tbListProduk = JsonConvert.DeserializeObject<Pesanan>(root.GetRawText());

                if (tbListProduk.Data.Header.Count() <= 0)
                {
                    writeLog($"Data Kosong");
                }

                return tbListProduk;
			}
			catch (Exception ex)
			{
				if (ex is OperationCanceledException)
				{
					writeLog("Gagal Baca Data : Request ke API terlalu lama (timeout).");
				}
				else if (ex is HttpRequestException)
				{
					writeLog($"Gagal Baca Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
				}
				else
				{
					writeLog($"Gagal Baca Data : Terjadi error: {ex.Message}");
				}
			}

			return tbListProduk;
		}

		public async Task<Pesanan> GetPesananApi(ParameterApi urlParam)
        {
            var config = await initConfigApi(urlParam);
            var tbListProduk = new List<Pesanan>();
            var tbListHead = new Pesanan();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                if (checking.statusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        urlParam.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(urlParam);

                        response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                        if (!response.IsSuccessStatusCode)
                        {
                            return tbListHead;
                        }
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }

                var json = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                tbListHead = JsonConvert.DeserializeObject<Pesanan>(root.GetRawText());
                if (tbListHead.Data.Header.Count() <= 0)
                {
                    writeLog($"Data Kosong");
                }
                ShowConsoleData(tbListHead);
                return tbListHead;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    writeLog("Gagal Baca Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    writeLog($"Gagal Baca Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    writeLog($"Gagal Baca Data : Terjadi error: {ex.Message}");
                }
            }

            return tbListHead;

        }


        public async Task<bool> LoginUserApi(ParameterApi urlParam, LoginViewModel data)
        {
            Dictionary<string, List<string>> masUserMenu = new Dictionary<string, List<string>>();

            var config = await initConfigApi(urlParam);
            var tbListProduk = new DataUser();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var requestBody = new { Telepon = data.NoTlp, Password = data.Password };

            var jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await config.httpReq.PostAsync(config.UrlApi, content, config.cancelTimer.Token);
                var checking = await CheckResponseAPI(response, config.UrlApi, "Login");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;
                    var dataLogin = GlobalDataService?.GlobalData?.ListDataLogin.FirstOrDefault(x => x.Telepon == data.NoTlp);

                    root.TryGetProperty("No_Id", out var No_Id);
                    root.TryGetProperty("Nama_Display", out var Nama_Display);
                    root.TryGetProperty("token", out var token);
                    root.TryGetProperty("refreshToken", out var refreshToken);

                    if (dataLogin != null)
                    {
                        dataLogin.token = token.ToString();
                        dataLogin.refreshToken = refreshToken.ToString();
                        dataLogin.No_Id = No_Id.ToString();
                        dataLogin.Nama_Display = token.ToString();
                        dataLogin.TanggalRefresh = DateTime.UtcNow.ToString("O");
                        dataLogin.Telepon = data.NoTlp.ToString();

                        ScopedDataServices.ScopedData.DataLogin = dataLogin;
                    }
                    else
                    {
                        GlobalDataService.GlobalData.ListDataLogin.Add(new DataLogin
                        {
                            token = token.ToString(),
                            Nama_Display = Nama_Display.ToString(),
                            refreshToken = refreshToken.ToString(),
                            Telepon = data.NoTlp.ToString(),
                            TanggalRefresh = DateTime.UtcNow.ToString("O"),
                            No_Id = No_Id.ToString()
                        });

                        ScopedDataServices.ScopedData.DataLogin = new DataLogin
                        {
                            token = token.ToString(),
                            Nama_Display = Nama_Display.ToString(),
                            refreshToken = refreshToken.ToString(),
                            Telepon = data.NoTlp.ToString(),
                            TanggalRefresh = DateTime.UtcNow.ToString("O"),
                            No_Id = No_Id.ToString()
                        };
                    }


                    masUserMenu.Add(data.NoTlp, new List<string> { "Akses" });

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.OtherPhone, data.NoTlp),
                        new Claim(ClaimTypes.PrimarySid, data.Password),
                        new Claim(ClaimTypes.Sid, No_Id.ToString()),
                        new Claim(ClaimTypes.Name, Nama_Display.ToString()),
                    };

                    var thisUsersAccountPolicies = masUserMenu[data.NoTlp];

                    foreach (var userAccountPolicy in thisUsersAccountPolicies)
                    {
                        claims.Add(new Claim(userAccountPolicy, "true"));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext?.SignInAsync(principal);

                    return true;
                }

            }
            catch (Exception ex)
            {
                writeLog($"Regist error: {ex.Message}");
            }

            return false;
        }



        public async Task<bool> CheckUserRegApi(ParameterApi urlParam, LoginViewModel data)
        {
            Dictionary<string, List<string>> masUserMenu = new Dictionary<string, List<string>>();

            var config = await initConfigApi(urlParam);
            var tbListProduk = new DataUser();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var requestBody = new { Telepon = data.NoTlp};

            var jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await config.httpReq.PostAsync(config.UrlApi, content, config.cancelTimer.Token);
                var checking = await CheckResponseAPI(response, config.UrlApi, "Check-Phone");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    root.TryGetProperty("can_register", out var Info);

                    return Convert.ToBoolean(Info.ToString());
                }

            }
            catch (Exception ex)
            {
                writeLog($"Regist error: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> CheckLoginApi(ParameterApi urlParam, LoginViewModel data)
        {
            Dictionary<string, List<string>> masUserMenu = new Dictionary<string, List<string>>();

            var config = await initConfigApi(urlParam);
            var tbListProduk = new DataUser();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var requestBody = new { Telepon = data.NoTlp, Password = data.Password, Jenis_User = data.Jenis_User };

            var jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await config.httpReq.PostAsync(config.UrlApi, content, config.cancelTimer.Token);
                var checking = await CheckResponseAPI(response, config.UrlApi, "Check-Login");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;
                    var dataLogin = GlobalDataService?.GlobalData?.ListDataLogin.FirstOrDefault(x => x.Telepon == data.NoTlp);

                    root.TryGetProperty("No_Id", out var No_Id);
                    root.TryGetProperty("Nama_Display", out var Nama_Display);
                    root.TryGetProperty("token", out var token);
                    root.TryGetProperty("refreshToken", out var refreshToken);

                    if (dataLogin != null)
                    {
                        dataLogin.token = token.ToString();
                        dataLogin.refreshToken = refreshToken.ToString();
                        dataLogin.No_Id = No_Id.ToString();
                        dataLogin.Nama_Display = token.ToString();
                        dataLogin.TanggalRefresh = DateTime.UtcNow.ToString("O");
                        dataLogin.Telepon = data.NoTlp.ToString();

                        ScopedDataServices.ScopedData.DataLogin = dataLogin;
                    }
                    else
                    {
                        GlobalDataService.GlobalData.ListDataLogin.Add(new DataLogin
                        {
                            token = token.ToString(),
                            Nama_Display = Nama_Display.ToString(),
                            refreshToken = refreshToken.ToString(),
                            Telepon = data.NoTlp.ToString(),
                            TanggalRefresh = DateTime.UtcNow.ToString("O"),
                            No_Id = No_Id.ToString()
                        });

                        ScopedDataServices.ScopedData.DataLogin = new DataLogin
                        {
                            token = token.ToString(),
                            Nama_Display = Nama_Display.ToString(),
                            refreshToken = refreshToken.ToString(),
                            Telepon = data.NoTlp.ToString(),
                            TanggalRefresh = DateTime.UtcNow.ToString("O"),
                            No_Id = No_Id.ToString()
                        };
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                writeLog($"Regist error: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> CreateSessionWeb(string no_id)
        {
            Dictionary<string, List<string>> masUserMenu = new Dictionary<string, List<string>>();

            var dataLogin = GlobalDataService?.GlobalData?.ListDataLogin.FirstOrDefault(x => x.No_Id == no_id);

            try
            {
                masUserMenu.Add(dataLogin.Telepon, new List<string> { "Akses" });

                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.OtherPhone, dataLogin.Telepon),
                        new Claim(ClaimTypes.PrimarySid, dataLogin.Password),
                        new Claim(ClaimTypes.Sid, dataLogin.No_Id.ToString()),
                        new Claim(ClaimTypes.Name, dataLogin.Nama_Display.ToString()),
                    };

                var thisUsersAccountPolicies = masUserMenu[dataLogin.Telepon];

                foreach (var userAccountPolicy in thisUsersAccountPolicies)
                {
                    claims.Add(new Claim(userAccountPolicy, "true"));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext?.SignInAsync(principal);

                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task RefreshDataMainKonten()
        {
            var produkTask = GetListGlobalDataAsync<Produk>(new ParameterApi
            {
                routeApi = "produk",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 12, show_variant = true, max_media = 2, status = "aktif", orderBy = "tgl_created", orderDir = "desc" }
            });

            var bannerTask = GetListGlobalDataAsync<Banner>(new ParameterApi
            {
                routeApi = "banner",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 3, orderBy = "nama_banner", orderDir = "desc", status = "aktif" }
            });

            var brandTask = GetListGlobalDataAsync<Brand>(new ParameterApi
            {
                routeApi = "brand",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { orderBy = "nama_brand", orderDir = "desc", status = "aktif" }
            });

            var kategoriTask = GetListGlobalDataAsync<Kategori>(new ParameterApi
            {
                routeApi = "kategori",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 3, status = "aktif" }
            });

            var companyTask = GetListGlobalDataAsync<Company>(new ParameterApi
            {
                routeApi = "company",
                prosesApi = "get-profile",
                ParamUrl = new ParameterApi.ParameterUrl { no_id = "comp_1" }
            });
            

            var NavbarTask = GetListGlobalDataAsync<Navbar>(new ParameterApi
            {
                routeApi = "navbar",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_id = "comp_1",
                    status = "aktif"
                }
            });

            var SosmedTask = GetListGlobalDataAsync<CompanySosial>(new ParameterApi
            {
                routeApi = "company",
                prosesApi = "get-social",
                ParamUrl = new ParameterApi.ParameterUrl
                {
                    no_id = "comp_1",
                    status = "aktif"
                }
            });

            var TeksTask = GetListGlobalDataAsync<TextSection>(new ParameterApi
            {
                routeApi = "teks",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl {}
            });

                        
            // Tunggu semua task selesai
            await Task.WhenAll(produkTask, brandTask, kategoriTask, companyTask, NavbarTask, SosmedTask);
        }

        public async Task RefreshDataAuthKonten()
        {
            if (ScopedDataServices.ScopedData.DataLogin != null && ScopedDataServices?.ScopedData?.DataLogin?.No_Id != "")
            {
                var CartTask = GetApi<Cart>(new ParameterApi
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
                        orderDir = "desc",
                        status = "aktif"
                    }
                });

                var SummaryPesananTask = GetApi<Pesanan.SummaryResponse>(new ParameterApi
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

                var nilaiDefaultTask = GetListGlobalDataAsync<NilaiDefault>(new ParameterApi
                {
                    routeApi = "nilaidefault",
                    prosesApi = "getAll",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                        no_id = "comp_1"
                    }
                });

                var metodeBayarTask = GetListGlobalDataAsync<Pembayaran>(new ParameterApi
                {
                    routeApi = "payment",
                    prosesApi = "option",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                        orderBy = "fee",
                        orderDir = "asc",
                    }
                });

                var metodeKirimTask = GetListGlobalDataAsync<Pengiriman>(new ParameterApi
                {
                    routeApi = "shipping",
                    prosesApi = "getAll",
                    token = ScopedDataServices.ScopedData.DataLogin.token,
                    refreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken,
                    ParamUrl = new ParameterApi.ParameterUrl
                    {
                        status = "aktif",
                        orderBy = "fee",
                        orderDir = "asc",
                    }

                });

                var infoCustomer = GetApi<DataUser>(new ParameterApi
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

                await Task.WhenAll(CartTask, SummaryPesananTask, nilaiDefaultTask, metodeKirimTask, metodeBayarTask, infoCustomer);

                if (infoCustomer.Result?.Data == null)
                {
                    NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    return;
                }

                ScopedDataServices.ScopedData.ListDataSummaryPesanan = SummaryPesananTask.Result?.Data ?? new();
                ScopedDataServices.ScopedData.ListDataCart = CartTask.Result?.Data ?? new();
                ScopedDataServices.ScopedData.DataLogin.DataUser = infoCustomer.Result?.Data?.FirstOrDefault() ?? new();
                ScopedDataServices.ScopedData.DataLogin.DataUser.Password = "xample_pass";

                var data = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == ScopedDataServices?.ScopedData?.DataLogin?.No_Id);
                data.DataUser = infoCustomer.Result?.Data?.FirstOrDefault() ?? new();
                data.DataUser.Password = "xample_pass";
            }
        }





        // Global API CALL
        // Global API CALL
        // Global API CALL

        //public async Task RefreshListGlobalDataAsync<T>(ParameterApi parameterApi)
        //{
        //    await _lock.WaitAsync();

        //    try
        //    {
        //        var ListNewData = await GetApi<T>(parameterApi);
        //        GlobalDataService.GlobalData.SetListData(typeof(T), ListNewData?.Data ?? new());

        //        GlobalDataService.GlobalStateChanged(); // beri tahu UI bahwa data diperbarui
        //    }
        //    finally
        //    {
        //        _lock.Release();
        //    }
        //}

        //public async Task RefreshSingelGlobalDataAsync<T>(ParameterApi parameterApi, bool force = false)
        //{

        //    var CurrentData = GlobalDataService.GlobalData.GetSingleData(typeof(T));
        //    if (CurrentData is IEnumerable list && list.Cast<object>().Any() && !force) return; // sudah ada, tidak perlu ambil lagi

        //    await _lock.WaitAsync();

        //    try
        //    {
        //        var NewData = await GetApi<T>(parameterApi);
        //        GlobalDataService.GlobalData.SetSingleData(typeof(T), NewData.Data.FirstOrDefault());
        //    }
        //    finally
        //    {
        //        _lock.Release();
        //    }
        //}





        public async Task GetListGlobalDataAsync<T>(ParameterApi parameterApi)
        {
            bool Updated = false;
            DateTime lastSync = new();

            await _lock.WaitAsync();

            try
            {
                if (GlobalDataService.GlobalData.ListPropertyMap.TryGetValue(typeof(T), out var propName))
                {
                    Updated = propName.Item2;
                    lastSync = propName.Item3;
                    var data = propName.Item1.GetType();

                    var CurrentData = GlobalDataService.GlobalData.GetListData(typeof(T));

                    if (parameterApi.Updated)
                    { 
                        var ListNewData = await GetApi<T>(parameterApi);
                        GlobalDataService.GlobalData.SetListData(typeof(T), ListNewData?.Data ?? new());
                    }
                    else if (CurrentData is IList list && list?.Count <= 0)
                    {
                        var ListNewData = await GetApi<T>(parameterApi);
                        GlobalDataService.GlobalData.SetListData(typeof(T), ListNewData?.Data ?? new());
                    }
                    else if (!Updated)
                    {
                        var ListNewData = await GetApi<T>(parameterApi);
                        GlobalDataService.GlobalData.SetListData(typeof(T), ListNewData?.Data ?? new());
                    }
                    else if (Updated && (DateTime.Now - lastSync).TotalMinutes >= 10)
                    {
                        var ListNewData = await GetApi<T>(parameterApi);
                        GlobalDataService.GlobalData.SetListData(typeof(T), ListNewData?.Data ?? new());
                    }
                }


                if (parameterApi.refreshAll) UpdateState();
                GlobalDataService.GlobalStateChanged();
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Server Eror Baca Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Server Eror Baca Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Server Eror Baca Data : Terjadi error: {ex.Message}");
                }
            }
            finally
            {
                _lock.Release();
            }

        }

        public async Task<bool> UpdateSingelGlobalDataAsync<T>(ParameterApi parameterApi, object DataUpdate, object no_id)
        {
            var status = false;
            await _lock.WaitAsync();
            try
            {
                var NewData = await UpdateApi<T>(parameterApi, DataUpdate);

                if (NewData == null)
                {
                    writeLog("Gagal Update Data : Response dari API kosong.");
                }
                else if (NewData.StatusCode == 200)
                {
                    GlobalDataService.GlobalData.SetSingleDataById(typeof(T), no_id, DataUpdate);
                    status = true;
                }
                else if (NewData.StatusCode == 401)
                {
                    writeLog("Gagal Update Data : Unauthorized. Token mungkin sudah kedaluwarsa.");
                }
                else
                {
                    writeLog("Gagal Update Data : Response dari API kosong.");
                }

                UpdateState();
                GlobalDataService.GlobalStateChanged();
            }
            finally
            {
                _lock.Release();
            }
            return status;
        }

        public async Task<bool> DeleteSingelGlobalDataAsync<T>(ParameterApi parameterApi, object DataUpdate, object no_id)
        {
            var status = false;
            await _lock.WaitAsync();
            try
            {
                var NewData = await DeleteApi<T>(parameterApi, DataUpdate);

                if (NewData == null)
                {
                    writeLog("Gagal Hapus Data : Response dari API kosong.");
                }
                else if (NewData.StatusCode == 200)
                {
                    GlobalDataService.GlobalData.DelSingleDataById(typeof(T), no_id, DataUpdate);
                    status = true;
                }
                else if (NewData.StatusCode == 401)
                {
                    writeLog("Gagal Hapus Data : Unauthorized. Token mungkin sudah kedaluwarsa.");
                }
                else
                {
                    writeLog("Gagal Hapus Data : Response dari API kosong.");
                }

                UpdateState();
                GlobalDataService.GlobalStateChanged();
            }
            catch
            {
                writeLog("Gagal Hapus Data : Terajadi Kesalahan Code.");
            }
            finally
            {
                _lock.Release();
            }
            return status;
        }

        public async Task<bool> AddSingelGlobalDataAsync<T>(ParameterApi parameterApi, object newData, object kode)
        {
            var status = false;
            await _lock.WaitAsync();
            try
            {
                var NewData = await CreateApi<T>(parameterApi, newData);

                if (NewData == null)
                {
                    writeLog("Gagal Tambah Data : Response dari API kosong.");
                }
                else if (NewData.StatusCode == 200)
                {
                    parameterApi.prosesApi = "getAll";
                    parameterApi.ParamUrl.kode = kode.ToString();
                    var newItem = await GetApi<T>(parameterApi);
                    GlobalDataService.GlobalData.AddSingleDataById(typeof(T), newItem.Data.FirstOrDefault() ?? newData);
                    status = true;
                }
                else if (NewData.StatusCode == 401)
                {
                    writeLog("Gagal Tambah Data : Unauthorized. Token mungkin sudah kedaluwarsa.");
                }
                else
                {
                    writeLog("Gagal Tambah Data : Response dari API kosong.");
                }

                UpdateState();
                GlobalDataService.GlobalStateChanged();
            }
            catch
            {
                writeLog("Gagal Tambah Data : Terajadi Kesalahan Code.");
            }
            finally
            {
                _lock.Release();
            }
            return status;
        }

        //public async Task<bool> ImportListGlobalDataAsync<T>(ParameterApi parameterApi, object newData)
        //{
        //    var status = false;
        //    await _lock.WaitAsync();
        //    try
        //    {
        //        var NewData = await ImportApi<T>(parameterApi, newData);

        //        if (NewData == null)
        //        {
        //            writeLog("Gagal Tambah Data : Response dari API kosong.");
        //        }
        //        else if (NewData.StatusCode == 200)
        //        {
        //            parameterApi.prosesApi = "getAll";
        //            parameterApi.ParamUrl.kode = "";
        //            var newItem = await GetApi<T>(parameterApi);
        //            GlobalDataService.GlobalData.SetListData(typeof(T), newItem.Data ?? new());
        //            status = true;
        //        }
        //        else if (NewData.StatusCode == 401)
        //        {
        //            writeLog("Gagal Tambah Data : Unauthorized. Token mungkin sudah kedaluwarsa.");
        //        }
        //        else
        //        {
        //            writeLog("Gagal Tambah Data : Response dari API kosong.");
        //        }

        //        UpdateState();
        //        GlobalDataService.GlobalStateChanged();
        //    }
        //    catch
        //    {
        //        writeLog("Gagal Tambah Data : Terajadi Kesalahan Code.");
        //    }
        //    finally
        //    {
        //        _lock.Release();
        //    }
        //    return status;
        //}










        // Scoped API CALL
        // Scoped API CALL
        // Scoped API CALL

        //public async Task RefreshListScopedDataAsync<T>(ParameterApi parameterApi, bool force = false)
        //{

        //    var CurrentDataList = ScopedDataServices.ScopedData.GetListData(typeof(T));
        //    if (CurrentDataList is IEnumerable list && list.Cast<object>().Any() && !force) return; // sudah ada, tidak perlu ambil lagi

        //    await _lock.WaitAsync();

        //    try
        //    {
        //        var ListNewData = await GetApi<T>(parameterApi);
        //        if (ListNewData != null)
        //        {
        //            ScopedDataServices.ScopedData.SetSingleData(typeof(T), ListNewData.Data);
        //        }

        //        UpdateState();
        //    }
        //    finally
        //    {
        //        _lock.Release();
        //    }
        //}

        //public async Task RefreshSingelScopedDataAsync<T>(ParameterApi parameterApi, bool force = false)
        //{

        //    var CurrentData = ScopedDataServices.ScopedData.GetSingleData(typeof(T));
        //    if (CurrentData is IEnumerable list && list.Cast<object>().Any() && !force) return; // sudah ada, tidak perlu ambil lagi
        //    object data = typeof(T);
        //    await _lock.WaitAsync();

        //    try
        //    {
        //        var NewData = await GetApi<T>(parameterApi);
        //        if (NewData != null)
        //        {
        //            ScopedDataServices.ScopedData.SetSingleData(typeof(T), NewData.Data.FirstOrDefault() ?? data);
        //        }

        //        UpdateState();
        //    }
        //    finally
        //    {
        //        _lock.Release();
        //    }
        //}




        private async void UpdateState()
        {
            await _lock.WaitAsync();

            try
            {
                //PdjService._hubConnection?.SendAsync("UpdateStateAllUser", ScopedDataServices.ScopedData.DataLogin.Kode_User);
            }
            finally
            {
                _lock.Release();
            }
        }




        public async Task<StatusResposeApi<T>> GetApi<T>(ParameterApi paramUrl)
        {
            paramUrl.ParamUrl.status = "aktif";
            var config = await initConfigApi(paramUrl);
            string url = config.UrlApi;
            StatusResposeApi<T> deserApi = new StatusResposeApi<T>();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);
                var checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                    return deserApi;
                }
                else if ((int)response.StatusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        paramUrl.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(paramUrl);

                        response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                            return deserApi;
                        }
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Baca Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Baca Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Baca Data : Terjadi error: {ex.Message}");
                }
            }
            return null;
        }

        public async Task<StatusResposeApi<T>> UpdateApi<T>(ParameterApi paramUrl, object DataUpdate)
        {
            var config = await initConfigApi(paramUrl);
            StatusResposeApi<T> deserApi = null;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                if (DataUpdate == null) return deserApi;
                var jsonContent = JsonConvert.SerializeObject(DataUpdate, jsonSettings);
                var response = await config.httpReq.PutAsync(config.UrlApi, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                var checking = await CheckResponseAPI(response, config.UrlApi, "Update");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                    return deserApi;
                }

                else if ((int)response.StatusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        paramUrl.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(paramUrl);

                        response = await config.httpReq.PutAsync(config.UrlApi, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "RefreshToken");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                            return deserApi;
                        }
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Update Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Update Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Update Data : Terjadi error: {ex.Message}");
                }
            }

            return null;
        }

        private async Task<StatusResposeApi<T>> DeleteApi<T>(ParameterApi paramUrl, object DataUpdate)
        {
            var config = await initConfigApi(paramUrl);
            StatusResposeApi<T> deserApi = null;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                if (DataUpdate == null) return deserApi;
                var jsonContent = JsonConvert.SerializeObject(DataUpdate, jsonSettings);
                var response = await config.httpReq.DeleteAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckResponseAPI(response, config.UrlApi, "Delete");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                    return deserApi;
                }

                else if ((int)response.StatusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        paramUrl.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(paramUrl);

                        response = await config.httpReq.DeleteAsync(config.UrlApi, config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "RefreshToken");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                            return deserApi;
                        }
                    }
                    else
                    {

                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Update Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Update Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Update Data : Terjadi error: {ex.Message}");
                }
            }

            return null;
        }

        private async Task<StatusResposeApi<T>> CreateApi<T>(ParameterApi paramUrl, object DataNew)
        {
            var config = await initConfigApi(paramUrl);
            StatusResposeApi<T> deserApi = null;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                if (DataNew == null) return deserApi;
                var jsonContent = JsonConvert.SerializeObject(DataNew, jsonSettings);
                var response = await config.httpReq.PostAsync(config.UrlApi, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                var checking = await CheckResponseAPI(response, config.UrlApi, "Create");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                    return deserApi;
                }

                else if ((int)response.StatusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        paramUrl.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(paramUrl);

                        response = await config.httpReq.PostAsync(config.UrlApi, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "RefreshToken");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            deserApi = JsonConvert.DeserializeObject<StatusResposeApi<T>>(json);
                            return deserApi;
                        }
                    }
                    else
                    {

                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Update Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Update Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Update Data : Terjadi error: {ex.Message}");
                }
            }

            return null;
        }




        public async Task<ChangePayment> ChangePayment(ParameterApi paramUrl, object DataUpdate)
        {
            var config = await initConfigApi(paramUrl);
            ChangePayment deserApi = null;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                if (DataUpdate == null) return deserApi;
                var jsonContent = JsonConvert.SerializeObject(DataUpdate, jsonSettings);
                var response = await config.httpReq.PostAsync(config.UrlApi, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                var checking = await CheckResponseAPI(response, config.UrlApi, "Update");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    deserApi = JsonConvert.DeserializeObject<ChangePayment>(json);
                    return deserApi;
                }

                else if ((int)response.StatusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        paramUrl.token = ScopedDataServices.ScopedData.DataLogin.token;
                        config = await initConfigApi(paramUrl);

                        response = await config.httpReq.PutAsync(config.UrlApi, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, config.UrlApi, "RefreshToken");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            deserApi = JsonConvert.DeserializeObject<ChangePayment>(json);
                            return deserApi;
                        }
                    }
                    else
                    {
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Update Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Update Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Update Data : Terjadi error: {ex.Message}");
                }
            }

            return null;
        }





        public async Task<bool> LogoutAsync(ParameterApi paramUrl)
        {
            var config = await initConfigApi(paramUrl);

            var url = "https://webapi.pdj.web.id/api/Auth/logout";

            object requestBody = new { RefreshToken = paramUrl.refreshToken };
            string jsonContent = "";

            HttpResponseMessage response;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try
            {
                jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
                response = await config.httpReq.PostAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                var checking = await CheckResponseAPI(response, url, "Logut");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                else if ((int)response.StatusCode == 401)
                {
                    var statusRefresh = await RefreshToken();

                    if (statusRefresh)
                    {
                        paramUrl.token = ScopedDataServices.ScopedData.DataLogin.token;
                        requestBody = new { RefreshToken = ScopedDataServices.ScopedData.DataLogin.refreshToken };

                        jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
                        config = await initConfigApi(paramUrl);

                        response = await config.httpReq.PostAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"), config.cancelTimer.Token);

                        checking = await CheckResponseAPI(response, url, "Logut");

                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Update Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Update Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Update Data : Terjadi error: {ex.Message}");
                }
            }

            return false;
        }

        public async Task<StatusResposeTransaksiApi<T>> GetTransaksiApi<T>(ParameterApi paramUrl)
        {
            var config = await initConfigApi(paramUrl);
            string url = config.UrlApi;
            StatusResposeTransaksiApi<T> deserApi = null;

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);
                var checking = await CheckResponseAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    deserApi = JsonConvert.DeserializeObject<StatusResposeTransaksiApi<T>>(json);

                    return deserApi;
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    await writeLog("Gagal Baca Data : Request ke API terlalu lama (timeout).");
                }
                else if (ex is HttpRequestException)
                {
                    await writeLog($"Gagal Baca Data : Terjadi kesalahan jaringan atau server. {ex.Message}");
                }
                else
                {
                    await writeLog($"Gagal Baca Data : Terjadi error: {ex.Message}");
                }
            }

            return deserApi;
        }



        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        private async Task<bool> RefreshToken()
        {
            var Pamramurl = new ParameterApi();
            object requestBody = null;
            await _refreshLock.WaitAsync(); // 🔒 Pastikan hanya 1 refresh berjalan
            try
            {
                var LoginUser = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == ScopedDataServices?.ScopedData?.DataLogin?.No_Id);
                if (LoginUser == null) return false;

                var cancelTimer = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var httpReq = new HttpClient();

                httpReq.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", LoginUser?.token);
                string defaultUrl = _config["ApiSettings:BaseUrl"];
                var url = $"{defaultUrl}Auth/refresh";

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                requestBody = new { refreshToken = LoginUser?.refreshToken };
                var jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);

                try
                {
                    var response = await httpReq.PostAsync(
                        url,
                        new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                        cancelTimer.Token
                    );

                    // Cek response validitas dulu
                    var checking = await CheckResponseAPI(response, url, "RefreshToken");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var deserApi = JsonConvert.DeserializeObject<StatusRefreshApi>(json);

                        if (deserApi == null)
                        {
                            await writeLog("Gagal Refresh Token: Deserialisasi response gagal.");
                            return false;
                        }

                        var cookieOption = new CookieOptions
                        {
                            HttpOnly = true,
                        };

                        LoginUser = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == ScopedDataServices?.ScopedData?.DataLogin?.No_Id);

                        LoginUser.token = deserApi.token ?? LoginUser.token;
                        LoginUser.refreshToken = deserApi.refreshToken ?? LoginUser?.refreshToken;
                        LoginUser.TanggalRefresh = DateTime.UtcNow.ToString("O");

                        ScopedDataServices.ScopedData.DataLogin.token = LoginUser.token;
                        ScopedDataServices.ScopedData.DataLogin.refreshToken = LoginUser.refreshToken;
                        ScopedDataServices.ScopedData.DataLogin.TanggalRefresh = LoginUser.TanggalRefresh;


                        await writeLog("✅ Refresh Token berhasil diperbarui.");
                        return true;
                    }
                    else
                    {
                        await writeLog($"Gagal Refresh Token: Status Code {response.StatusCode}");
                        NavigationManager.NavigateTo($"/auth-user-logout/logout", forceLoad: true);
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException or HttpRequestException)
                {
                    string msg = ex switch
                    {
                        OperationCanceledException => "Request ke API terlalu lama (timeout).",
                        HttpRequestException => $"Kesalahan jaringan atau server: {ex.Message}",
                        _ => $"Terjadi kesalahan: {ex.Message}"
                    };
                    await writeLog($"Gagal Refresh Token: {msg}");
                }
            }
            finally
            {
                _refreshLock.Release(); // 🔓 Lepaskan lock agar request lain bisa jalan
            }

            return false;
        }





        public async Task ShowConsoleData(object myData, string judul = "Perhatian...")
        {
            Console.WriteLine(judul);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(myData, new JsonSerializerOptions { WriteIndented = true }));
        }




        public static string ToQueryString<T>(T model)
        {
            if (model == null) return string.Empty;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var query = string.Join("&",
                properties
                    .Where(p => p.GetValue(model) != null)
                    .Select(p => {
                        var value = p.GetValue(model)?.ToString();
                        return $"{HttpUtility.UrlEncode(p.Name)}={value}";
                    })
            );

            return query;
        }

        public async Task<(HttpClient httpReq, CancellationTokenSource cancelTimer, string UrlApi)> initConfigApi(ParameterApi urlParam)
        {
            urlParam = urlParam ?? new ParameterApi();
            var httpReq = new HttpClient();
            var cancelTimer = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var LoginUser = GlobalDataService.GlobalData.ListDataLogin.FirstOrDefault(x => x.No_Id == ScopedDataServices?.ScopedData?.DataLogin?.No_Id);
            httpReq.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", LoginUser?.token);

            string defaultUrl = _config["ApiSettings:BaseUrl"];
            string UrlApi = defaultUrl + urlParam.routeApi + "/" + urlParam.prosesApi + "?" + ToQueryString(urlParam.ParamUrl);

            return (httpReq, cancelTimer, UrlApi);
        }



        public async Task<(int statusCode, bool statusResponse, string MessageInfo)> CheckResponseAPI(HttpResponseMessage response, string url, string? proses = "AMBIL")
        {
            var content = "";
            var MassageInfo = "";
            var token = "";
            var RefreshToken = "";
            StatusResponse result = new StatusResponse();

            try
            {
                if ((int)response.StatusCode == 204)
                {
                    content = "Data yang dicari tidak ditemukan/kosong";
                }
                else if (response.Content != null && response.Content.Headers.ContentLength != 0)
                {
                    var bodyContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(bodyContent))
                    {
                        result = System.Text.Json.JsonSerializer.Deserialize<StatusResponse>(bodyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        MassageInfo = result.Message;
                        token = result.token;
                        RefreshToken = result.refreshToken;
                    }
                    else
                    {
                        MassageInfo = "Kesalahan Permintaan";
                    }
                }
                else
                {
                    MassageInfo = "Permintaan Tidak Ditemukan";
                }

                if ((int)response.StatusCode == 401)
                {

                }


                if (response.IsSuccessStatusCode && (int)response.StatusCode != 204)
                {
                    content = $@"Success: {proses} - {MassageInfo}";
                }
                else if (proses == "Refresh")
                {
                    content = $@"Redirected: {proses} - {MassageInfo}";
                }
                else if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                {
                    content = $@"Redirected: {proses} - {MassageInfo}";
                }
                else if ((int)response.StatusCode >= 400 && (int)response.StatusCode <= 499)
                {
                    content = $@"Client error: {proses} - {MassageInfo}";
                }
                else if ((int)response.StatusCode >= 500)
                {
                    content = $@"Server error: {proses} - {MassageInfo}";
                }
            }
            catch
            {
                content = "Data yang dicari tidak ditemukan/kosong";
            }

            var log = $@"
                        [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]
                        URL: {url}
                        Status: {(int)response.StatusCode} {response.ReasonPhrase}
                        Response: {content}
                        ---------------------------------------------------------";

            var logDir = Path.Combine("Logs");
            var fileName = $"LogApi-{DateTime.Now:yyyy-MM-dd}.txt";
            var logPath = Path.Combine(logDir, fileName);

            Directory.CreateDirectory(logDir);

            using (var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(log);
            }

            return ((int)response.StatusCode, response.IsSuccessStatusCode, MassageInfo);
        }

        public async Task writeLog(string content)
        {
            var log = $@"
                        [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - Error API Connection
                        Message: {content}
                        ---------------------------------------------------------";

            var logDir = Path.Combine("Logs");
            var fileName = $"LogApi-{DateTime.Now:yyyy-MM-dd}.txt";
            var logPath = Path.Combine(logDir, fileName);

            Directory.CreateDirectory(logDir);

            using (var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(log);
            }
        }

    }


}