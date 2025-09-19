using Azure;
using Dapper;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json;

//using TradeWeb.Data;
using Npgsql;
//using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor.Markdown;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using static Services.pdjService;

namespace Services
{
    public partial class pdjService
    {
       
        [Inject]
        protected DialogService DialogService { get; set; }

      
        private readonly NavigationManager navigationManager;
        private readonly string connectionString;

        private readonly DialogService _dialogService;
        private readonly IJSRuntime _jsRuntime;

        private static Random _rand = new Random();

        public static List<T> GenerateDummy<T>(int jumlah) where T : class, new()
        {
            var list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < jumlah; i++)
            {
                var obj = new T();
                foreach (var prop in props)
                {
                    if (!prop.CanWrite) continue;

                    object? value = null;

                    if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                        value = _rand.Next(1, 1000);

                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                        value = Math.Round((decimal)_rand.NextDouble() * 100000, 2);

                    else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                        value = Math.Round(_rand.NextDouble() * 1000, 2);

                    else if (prop.PropertyType == typeof(string))
                        value = $"{prop.Name}_{i + 1}";

                    else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        value = DateTime.Now.AddDays(-_rand.Next(1, 100));

                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                        value = _rand.Next(0, 2) == 0;

                    prop.SetValue(obj, value);
                }
                list.Add(obj);
            }

            return list;
        }
        public class StatusResponse
        {
            public int StatusCode { get; set; } = 0;
            public string Message { get; set; } = "";
            public string NewId { get; set; } = "";
            public string NoBukti { get; set; } = "";
            public string token { get; set; } = "";
            public string refreshToken { get; set; } = "";
        }

        private string addParam(string paramName, string value, bool first = false)
        {
            if (first)
            {
                return "?" + paramName + "=" + value;
            }
            else
            {
                return "&" + paramName + "=" + value;
            }

        }


        public async Task<(int statusCode, bool statusResponse, string MessageInfo)> CheckAPI(HttpResponseMessage response, string url, string? proses = "AMBIL")
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

            return ((int)response.StatusCode, response.IsSuccessStatusCode, MassageInfo + "%section%" + result.NewId);
        }

        public void writeLog(string content)
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
                writer.WriteLineAsync(log);
            }
        }

        public static string ToQueryString<T>(T model)
        {
            if (model == null) return string.Empty;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var query = string.Join("&",
                properties
                    .Where(p => p.GetValue(model) != null) // hanya yang tidak null
                    .Select(p => $"{HttpUtility.UrlEncode(p.Name)}={HttpUtility.UrlEncode(p.GetValue(model)?.ToString())}")
            );

            return query;
        }

        public (HttpClient httpReq, CancellationTokenSource cancelTimer, string UrlApi) initConfigApi(ParameterApi urlParam)
        {
            urlParam = urlParam ?? new ParameterApi();
            var httpReq = new HttpClient();
            var cancelTimer = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
             
            string UrlApi = urlParam.defaultUrl + urlParam.routeApi + "/" + urlParam.prosesApi + "?" + ToQueryString(urlParam.ParamUrl); 

            return (httpReq, cancelTimer, UrlApi);
        }

        public async Task<List<Produk>> GetProdukApi(ParameterApi paramUrl)
        {
            var config = initConfigApi(paramUrl);
            string url = config.UrlApi ;
            var tbListProduk = new List<Produk>();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Produk>>(dataFromDb.GetRawText());
                    }
                    else
                    {
                        writeLog($"Data Kosong");
                    }
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
        
        public async Task<List<Kategori>> GetKategoriApi(ParameterApi urlParam)
        {
            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Kategori>();
            var tbListHead = new Kategori();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Kategori>>(dataFromDb.GetRawText());
                    }
                    if (root.TryGetProperty("Group_Kategori", out var groupFromDb) && groupFromDb.GetArrayLength() > 0)
                    {
                        tbListHead.Group_Kategori = JsonConvert.DeserializeObject<List<string>>(groupFromDb.GetRawText());
                        foreach (var detail in tbListProduk)
                        {
                            detail.Group_Kategori = tbListHead.Group_Kategori;
                        }
                    }
                    else
                    {
                        writeLog($"Data Kosong");
                    }
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

        public async Task<List<Cart>> GetCartApi(ParameterApi urlParam)

        {
            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Cart>();
            var tbListHead = new Kategori();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Cart>>(dataFromDb.GetRawText());
                    }
                    else
                    {
                        writeLog($"Data Kosong");
                    }
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

        public async Task<(string messageResponse, bool statusResponse, int codeNumber, string proses)> CudCartApi(string proses, Cart cart = null , ParameterApi urlParam = null )
        {
            var config = initConfigApi(urlParam);
            var result = (statusCode: 0, statusResponse: false, MessageInfo: "");
            object requestBody = null;
            string jsonContent = "";
            HttpResponseMessage response;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try { 
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

                result = await CheckAPI(response, config.UrlApi, proses);

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

        public async Task<(string messageResponse, bool statusResponse, int codeNumber, string proses)> CudPesananApi(string proses, object order = null , ParameterApi urlParam = null )
        {
            var config = initConfigApi(urlParam);
            var result = (statusCode: 0, statusResponse: false, MessageInfo: "");
            object requestBody = null;
            string jsonContent = "";
            HttpResponseMessage response;

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            try { 
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
                        jsonContent = JsonConvert.SerializeObject(requestBody, jsonSettings);
                        content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        response = await config.httpReq.PutAsync(config.UrlApi, content, config.cancelTimer.Token);
                        break;
                }

                result = await CheckAPI(response, config.UrlApi, proses);

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

        public async Task<List<Pengiriman>> GetMetodeKirimApi(ParameterApi urlParam)
        {

            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Pengiriman>();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Pengiriman>>(dataFromDb.GetRawText());
                    }
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

        public async Task<List<Pembayaran>> GetMetodeBayarApi(ParameterApi urlParam)
        {

            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Pembayaran>();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Pembayaran>>(dataFromDb.GetRawText());
                    }
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

        public async Task<List<Brand>> GetBrandApi(ParameterApi urlParam)
        {

            var config = initConfigApi(urlParam);
            string url = config.UrlApi + "brand/getall";

            //url += addParam("no_id", idProduk, true);
            var tbListProduk = new List<Brand>();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                { 
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Brand>>(dataFromDb.GetRawText());
                    }
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

        public async Task<List<Brand>> GetBannerApi(ParameterApi urlParam)
        {
            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Brand>();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Brand>>(dataFromDb.GetRawText());
                    }
                    else
                    {
                        writeLog($"Data Kosong");
                    }
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
            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Pesanan>();
            var tbListHead = new Pesanan();

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Data", out var dataFromDb))
                    {
                        tbListHead = JsonConvert.DeserializeObject<Pesanan>(dataFromDb.GetRawText());
                    }
                    else
                    {
                        writeLog($"Data Kosong");
                    }
                }

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

        public async Task<(List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount)> GetSearchApi(ParameterApi urlParam)

        {
            var config = initConfigApi(urlParam);
            var tbListProduk = new List<Produk>();
            var tbListBrand = new List<Brand>();
            int totalCount = 0;

            try
            {
                var response = await config.httpReq.GetAsync(config.UrlApi, config.cancelTimer.Token);

                var checking = await CheckAPI(response, config.UrlApi, "BACA");

                if (checking.statusResponse)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("Produk", out var dataFromDbProduk))
                    {
                        tbListProduk = JsonConvert.DeserializeObject<List<Produk>>(dataFromDbProduk.GetRawText());
                    }
                    if (root.TryGetProperty("Brand", out var dataFromDbBrand))
                    {
                        tbListBrand = JsonConvert.DeserializeObject<List<Brand>>(dataFromDbBrand.GetRawText());
                    }
                    if (root.TryGetProperty("TotalCount", out var dataFromDbTotalCount))
                    {
                        totalCount = JsonConvert.DeserializeObject<int>(dataFromDbTotalCount.GetRawText());
                    }
                    else
                    {
                        writeLog($"Data Kosong");
                    }
                }
                return (tbListBrand, tbListProduk, totalCount);
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

            return (tbListBrand, tbListProduk, totalCount);
        }
























        // Company Setting 
        public string clientLogo = "images/logo_client_company.png";
        public const string welcomeBackground = "images/lembah_metro.jpg";
        public string clientName = "PT. ABC INDONESIA";
        public string jumlahDecimalQty = "N2";
        public string jumlahDecimalRp = "N2";
        public string jumlahDecimalPersen = "N2";

       
        public int INDEX_HEADER = 0;
        public int INDEX_STOCK = 1;
        public int INDEX_NONSTOCK = 2;
        public int INDEX_BATAL = 3;
        public int INDEX_SELESAI = 4;

        public const int INDEX_DEFAULT = 1;

        // ==


        // General
        public bool showUpDown = false;
        public const int screenBreakpoint = 1024;
        public string styleButtonIcon = "background-color: transparent; color: #1e3a6a";
        public string pageFrameStack = "flex-grow: 1; margin-top: 0.5em; width: 100%; overflow: auto; gap: 0; padding: 1.25rem;";
        public string judulHalaman = "margin-right: 0.75rem; color: black; margin-top: 0.25em";
       
        public string BtnRefreshStyle = "background-color: transparent; color: #1e3a6a;  box-shadow: 0px 0px 4px rgba(0, 0, 0, 0.2); text-align: center !important; ";
        public const string btnGradientStyle = "background: linear-gradient(90deg, #339ce6 0%, #083654 100%); color: #FFFFFF; border: none; border-radius: 4px; box-shadow: 0px 4px 6px rgba(0, 0, 0, 0.1);";
        public string BtnTambahStyleEnabledSO = "margin-right: 5px; border-radius: 4px;font-weight: 600; padding-top: 3.5px; padding-bottom: 3.5px; position: absolute; right: 0px; background: #339ce6; color: #FFFFFF; border: none;";
        public string BtnTambahStyleEnabledSOSmallScreen = "border-radius: 4px; background: #339ce6; color: #FFFFFF; border: none; margin-bottom: 1rem;";

        public string BtnTambahStyleEnabledMutasi = "bottom: 0px;" + "margin-right: 5px; border-radius: 4px;font-weight: 600; padding-top: 3.5px; padding-bottom: 3.5px; position: absolute; right: 0px; background: #339ce6; color: #FFFFFF; border: none;";
        public string BtnTambahStyleEnabledMutasiSmallScreen = "position: absolute!important; right: 0px !important; bottom: 0px !important;" + "border-radius: 4px; background: #339ce6; color: #FFFFFF; border: none; margin-bottom: 0;";
        public string WrapperBtnTambahMutasi = "position: relative; height: 5.9em; ";
        public string WrapperBtnTambahMutasiSmallScreen = "position: relative; height: 4.68em; ";

        public string tabsCardSO = "margin-top: 0; padding: 0!important; margin: 0; height: 100%; width: calc(100% - 1em) !important;";
        public string tabsCardSOPhone = "margin-top: 0; padding: 0!important; margin: 0; height: 100%; width: 100% !important";
        // ==

        // Subtotal
        public string fakturSubtotal = "width: 35%; text-align: left; font-size: 18px;"; // 15px;";
        public string fakturRp = "width: 5%; text-align: left; font-size: 18px;"; // 15px;";
        public string fakturNominal = "width: 60%; text-align: right; font-size: 18px;"; // 15px;";
        public string fakturSubtotalPhone = "width: 35%; text-align: left; font-size: 15px;";
        public string fakturRpPhone = "width: 5%; text-align: left; font-size: 15px;";
        public string fakturNominalPhone = "width: 60%; text-align: right; font-size: 15px;";
        // ==

        // Total Akhir
        public string fakturTotal = "width: 35%; text-align: left; font-size: 22px;"; // 19px;";
        public string fakturTotalRp = "width: 5%; text-align: left; font-size: 20px;"; // 17px;";
        public string fakturTotalNominal = "width: 60%; text-align: right; font-size: 22px;"; // 19px;";
        public string fakturTotalPhone = "width: 35%; text-align: left; font-size: 19px;";
        public string fakturTotalRpPhone = "width: 5%; text-align: left; font-size: 17px;";
        public string fakturTotalNominalPhone = "width: 60%; text-align: right; font-size: 19px;";
                                                                                              // ==

        // Grid transaksi
        //  public string divUntukGridSO = "overflow: auto; height: 38.3vh; padding: 1.25rem;box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); background-color: #FFFFFF;";

        public string divUntukGridSO = "overflow: auto; height: 38.3vh; padding: 1.25rem; box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);";
        public string divUntukGridMutasi = "overflow: auto; height: 50vh; padding-top: 0.5rem; padding-bottom: 0rem; padding-left: 0; padding-right: 0rem";
        // ==

        // Icon







        public const string prevIcon = "chevron_left";
        public string btnPrevIcon = prevIcon;

        public const string searchIcon = "search";
        public string btnSearchIcon = "search";

        public const string nextIcon = "chevron_right";
        public string btnNextIcon = "chevron_right";

        public const string refreshIcon = "sync";
        public string btnRefreshIcon = "sync";

        public const string batalIcon = "cancel";
        public string btnBatalIcon = batalIcon;

        public const string cetakIcon = "print";
        public string btnCetakIcon = cetakIcon;

        public const string tambahIcon = "add_circle_outline";
        public string btnTambahIcon = tambahIcon;

        public const string saveIcon = "save";
        public string btnSaveIcon = saveIcon;

        public const string editIcon = "edit";
        public string btnEditIcon = editIcon;

        public const string hapusIcon = "delete";
        public string btnHapusIcon = hapusIcon;

        public const string selesaiManualIcon = "check_circle";
        public string btnSelesaiManualIcon = selesaiManualIcon;
        // ===

        // Grid
        public const string backgroundGridGanjil = "background-color: #f4f5f7";
        public const string backgroundGridGenap = "background-color: #ffffff";
        public const string backgroundGridPink = "background-color: #FFE0E0";
        public const string backgroundGridLightGreen = "background-color: #E0FFE0";
        public const string backgroundGridHeader = "border-color: rgba(255,255,255, 0.5); padding-top: 0.95em; padding-bottom: 0.95em; text-align: center";
        public const string backgroundGridHeader2 = "background-color: #660066; border-color: rgba(255,255,255, 0.5); padding-top: 0.95em; padding-bottom: 0.95em; text-align: center";

        public const string backgroundGridHeaderMerah = "background-color: #D32F2F !important; color: #ffffff;padding-top: 0.95em; padding-bottom: 0.95em; text-align: center; ";
        // ===

        // Dialog Title
        public const string judulDialogSearch = "History";
        public const string judulDialogTambahItem = "Tambah Item";
        public const string judulDialogEditItem = "Koreksi / Hapus Item";


        // ==

        // Dialog Dimension & Search Page, 1 = normal screen, 2 = phone
        public string maksOrderQtyStyle = "color: green";
        public string captionBatalGridColumnHeader = "Batal ?";

        public string dialogSmallVh = "height: 27vh !important; width: 100% !important; padding: 0; margin: 0; overflow-y: auto;";
        public string dialogMediumVh = "height: 57vh !important; width: 100% !important; padding: 0; margin: 0; overflow-y: auto;";

        public string dialogLargeVh = "height: 70vh !important; width: 100% !important; padding: 0; margin: 0; overflow-y: auto;";

        public string dialogHeightSODetil = "70%";
        public string dialogWidthSODetil = "30%";
        public string dialogHeightMutasiDetil = "40%";
        public string dialogWidthMutasiDetil = "30%";

        public const string SearchPageStyle = "overflow: auto; height: 55vh !important;  padding: 0 !important; margin: 0 !important";
        public const string SearchPageStylePhoneLandscape = "overflow: auto; height: 80vh !important;  padding: 0 !important; margin: 0 !important";
        public string SearchStack = "margin: 0 !important; padding: 0 !important;  height: 100% !important; overflow: hidden !important";
        public string SearchInput = "font-size: 15px; width: 100%; margin-bottom: 0.75em";
        public string SearchStack2 = "height: calc(100% - 4.75em);";
        public string PencarianText = "Pencarian";

        public string DetilBtnHapusStyle = "padding-left: 0.5em!important; padding-right: 0.5em!important; color: #e53e3e";
        public string DetilBtnHapusText = "Hapus";
        public string DetilBtnBatalStyle = "background-color: transparent !important; color: #1e3a6a !important; padding-left: 2em !important; padding-right: 2em !important";
        public string DetilBtnBatalText = "Batal";
        public string DetilBtnSimpanStyle = "padding-left: 2em !important; padding-right: 2em !important";
        public string DetilBtnSimpanText = "Simpan";

        // ===


        // Dialog Konfirmasi
        public const string yakinHapus = "Yakin hapus data?";
        public const string judulDialogKonfirmasi = "Konfirmasi...";
        public const string yes = "Ya";
        public const string tidak = "Batal";
        // ==

        // Input
        public string HeaderInfoStyleEnabled = "width: 100% !important; font-size: 15px!important; height: 32px;!important; background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string HeaderInfoStyleDisabled = "width: 100% !important; font-size: 15px!important; height: 32px!important; background-color: #E0E0E0!important; color: black!important; opacity: 0.8!important; box-shadow: none!important;";
        public string DropDownHeaderInfoStyleEnabled = "width: 100%!important; font-size: 15px; height: 32px; line-height: 1.45; padding: 0.25rem;background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string DropDownHeaderInfoStyleDisabled = "width: 100%!important; font-size: 15px; height: 32px; background-color: #E0E0E0 !important; color: black; line-height: 1.45; padding: 0.25rem; opacity: 0.8";
        public string DateHeaderInfoStyleEnabled = "width: 100% !important; font-size: 15px!important; height: 34px;!important; background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string DateHeaderInfoStyleDisabled = "width: 100% !important; font-size: 15px!important; height: 34px!important; background-color: #E0E0E0!important; color: black!important; opacity: 0.8!important; box-shadow: none!important;";
        public string HeaderInfoStyleMemoDisabled = "width: 100%!important; font-size: 15px; height: 78px; background-color: #E0E0E0!important; color: black; opacity: 0.8";
        public string HeaderInfoStyleMemoEnabled = "width: 100%!important; font-size: 15px; height: 78px; background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string AlasanBatalDisabled = "width: 100% !important;font-size: 15px !important; height: 32px !important; background-color: rgba(255, 99, 71, 0.1) !important;  color: #333 !important; border: 0px solid rgba(255, 99, 71, 0.3) !important; border-radius: 0px; padding: 4px 8px; box-sizing: border-box; ";
       
        /*public string HeaderInfoStyleEnabled = "width: 100%; font-size: 14px!important; height: 28px;!important; background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string HeaderInfoStyleDisabled = "width: 100% !important; font-size: 14px!important; height: 28px!important; background-color: #E0E0E0!important; color: black!important; opacity: 0.8!important; box-shadow: none!important;";
        public string DropDownHeaderInfoStyleEnabled = "width: 100%; font-size: 14px; height: 28px; line-height: 1.45; padding: 0.25rem;background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string DropDownHeaderInfoStyleDisabled = "width: 100%; font-size: 14px; height: 28px; background-color: #E0E0E0 !important; color: black; line-height: 1.45; padding: 0.25rem; opacity: 0.8";
        public string DateHeaderInfoStyleEnabled = "width: 100% !important; font-size: 14px!important; height: 30px;!important; background-color: rgba(208, 226, 240, 0.6) !important; border: 1px #b3c8db solid !important; box-shadow: none!important;";
        public string DateHeaderInfoStyleDisabled = "width: 100% !important; font-size: 14px!important; height: 30px!important; background-color: #E0E0E0!important; color: black!important; opacity: 0.8!important; box-shadow: none!important;";
        public string HeaderInfoStyleMemoDisabled = "width: 100%; font-size: 14px; height: 99px; background-color: #E0E0E0!important; color: black; opacity: 0.8";
        public string HeaderInfoStyleMemoEnabled = "width: 100%; font-size: 14px; height: 99px; background-color: rgba(208, 226, 240, 0.3) !important; border: 0.5px rgba(208, 226, 240, 0.8) solid !important";
        public string AlasanBatalDisabled = "width: 100% !important;font-size: 14px !important; height: 28px !important; background-color: rgba(255, 99, 71, 0.1) !important;  color: #333 !important; border: 0px solid rgba(255, 99, 71, 0.3) !important; border-radius: 0px; padding: 4px 8px; box-sizing: border-box; ";
        */
        // ==

        // No bukti & Batal
        public string NoBuktiStyle = "display: flex; align-items: center; font-size: 1.5rem; color: #1e3a6a; margin: 0";
        public string batalSmallScreen = "margin-left: 1em; width: auto;  color: #e53e3e; font-size:1.2rem;font-weight: bold; padding: 0; text-align: center !important ";
        public string textBatal = "B A T A L";
        public string batalNormalScreen = "color: #e53e3e; font-size:1.4rem;font-weight: bold; padding: 0; text-align: center !important;  margin-left: 1em; margin-bottom: 0 !important; margin-top: 0;  ";
        public string styleAlasanBatal = "color: red !important; font-weight: 600";

        public string styleAlasanBatal2 = "background-color: #E0E0E0; color: #D93232 !important; font-weight: 600;width: 100% !important; font-size: 15px!important; height: 34px!important; opacity: 0.8!important; box-shadow: none!important;";
        //  #D32F2F #F44336  

        public string styleAlasanSelesai2 = "background-color: #E0E0E0; color: #008F47 !important; font-weight: 600;width: 100% !important; font-size: 15px!important; height: 34px!important; opacity: 0.8!important; box-shadow: none!important;";
        // #4CAF50
        // ==









        private readonly AuthenticationStateProvider authProvider;

        public pdjService(NavigationManager navigationManager, DialogService dialogService, IJSRuntime jsRuntime, AuthenticationStateProvider authProvider)
        {

            this.navigationManager = navigationManager;
            _dialogService = dialogService; // Initialize here
            _jsRuntime = jsRuntime;
            this.authProvider = authProvider;

        }

        public async Task<bool> isThisUser(string name)
        {
            var authState = await authProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            string username = user.Identity?.Name;

            if (username == name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

       
       


        public async Task SelectAllText(string elementId)
        {

            await _jsRuntime.InvokeVoidAsync("selectAllText", elementId);
        }
        public async Task SetFocusAndSelect(string elementId)
        {

            await _jsRuntime.InvokeVoidAsync("setFocusAndSelect", elementId);
        }
        public async Task SetDropdownFocus(string elementId)
        {

            await _jsRuntime.InvokeVoidAsync("setDropdownFocus", elementId);
        }
               
      
        public async Task ShowNotification(string myMessage, string judul = "Perhatian...")
        {

            await _dialogService.Alert(myMessage, judul, new AlertOptions { OkButtonText = "OK", CloseDialogOnOverlayClick = true, Style = "", CssClass = "" });

        }
      
        public async Task ShowNotificationData(object myData, string judul = "Perhatian...")
        {

            await _dialogService.Alert(System.Text.Json.JsonSerializer.Serialize(myData, new JsonSerializerOptions { WriteIndented = true }), judul, new AlertOptions { OkButtonText = "OK", CloseDialogOnOverlayClick = true, Style = "", CssClass = "wrap-alert-text" });
        }

        public async Task ShowConsoleData(object myData, string judul = "Perhatian...")
        {
            Console.WriteLine(judul);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(myData, new JsonSerializerOptions { WriteIndented = true }));
        }
    }


}
