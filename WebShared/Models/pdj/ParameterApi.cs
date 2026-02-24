using System.Collections;


namespace WebShared.Models
{
    public class StatusResposeApi<T>
    {
        public int StatusCode { get; set; } = 0;
        public int TotalCount { get; set; } = 0;
        public string LastCursor { get; set; } = "";
        public string Message { get; set; } = "";
        public string No_Bukti { get; set; } = "";
        public string token { get; set; } = "";
        public bool? HasMore { get; set; } = null;
        public string refreshToken { get; set; } = "";
        public List<T> Data { get; set; } = new();
        public Meta meta { get; set; } = new();
    }

    public class StatusResposeApiHomepage<T> 
    {
        public int StatusCode { get; set; } = 0;
        public int TotalCount { get; set; } = 0;
        public string LastCursor { get; set; } = "";
        public string Message { get; set; } = "";
        public string No_Bukti { get; set; } = "";
        public string token { get; set; } = "";
        public bool? HasMore { get; set; } = null;
        public bool StatusGet { get; set; } = false;
        public string refreshToken { get; set; } = "";
        public T Data { get; set; }
    }

    public class Meta
    {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = 0;
        public int Total { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
    }

    public class StatusRefreshApi
    {
        public int StatusCode { get; set; } = 0;
        public string Message { get; set; } = "";
        public string No_Bukti { get; set; } = "";
        public string token { get; set; } = "";
        public string refreshToken { get; set; } = "";
    }

    public class DataHomePage
    {
        public List<Navbar> Navbar { get; set; } = new List<Navbar>();
        public List<Banner> Banners { get; set; } = new List<Banner>();
        public List<Brand> Brands { get; set; } = new List<Brand>();
        public List<Kategori> Categories { get; set; } = new List<Kategori>();
        public List<TextSection> Texts { get; set; } = new List<TextSection>();
        public List<Produk> Products { get; set; } = new List<Produk>();
        public Company Company { get; set; } = new Company();
        public List<CompanySosial> Socials { get; set; } = new List<CompanySosial>();
        public bool Status { get; set; } = false;

        public void UpdateFrom(StatusResposeApiHomepage<DataHomePage> Source)
        {
            if (Source?.Data == null)
            {
                Status = false;
                return;
            }

            Status = Source?.StatusGet ?? false;
            Navbar = Source.Data.Navbar ?? new();
            Banners = Source.Data.Banners ?? new();
            Brands = Source.Data.Brands ?? new();
            Categories = Source.Data.Categories ?? new();
            Texts = Source.Data.Texts ?? new();
            Products = Source.Data.Products ?? new();
            Company = Source.Data.Company ?? new();
            Socials = Source.Data.Socials ?? new();
        }
    }

    public class ChangePayment
    {
        public string Message { get; set; } = "";
        public string StatusLama { get; set; } = "";
        public CancelResults CancelResult { get; set; } = new();
    }

    public class CancelResults
    {
        public int Status { get; set; } = 0;
        public string Message { get; set; } = "";
        public Pesanan.MidtransResponses Data { get; set; } = new ();
    }

    public class StatusResposeTransaksiApi<T>
    {
        public int StatusCode { get; set; } = 0;
        public string Message { get; set; } = "";
        public string No_Bukti { get; set; } = "";
        public string token { get; set; } = "";
        public string refreshToken { get; set; } = "";
        public T? Data { get; set; }
    }




    public class TypeActive<T> where T : new()
    {
        public string Judul { get; set; } = "";
        public string Info { get; set; } = "";
        public Type DataType { get; set; } = typeof(T);
        public object Object { get; set; } = new();
        public IList ObjectList { get; set; } = new ArrayList();
        public T DataByType { get; set; } = new();
        public List<T> ListDataByType { get; set; } = new();
    }

    public partial class ParameterApi
        {
            public string defaultUrl { get; set; } = "http://26.75.164.107:87/api/";
            public string routeApi { get; set; } = "";
            public string prosesApi { get; set; } = "";
            public string token { get; set; } = "";
            public string refreshToken { get; set; } = "";
            public string lastRefresh { get; set; } = "";
            public bool transaksi { get; set; } = false;
            public bool Updated { get; set; } = false;
            public bool refreshAll { get; set; } = false;
            public Type typeTarget { get; set; } = null;
            public ParameterUrl ParamUrl { get; set; }


            public class ParameterUrl
            {
                public string no_id { get; set; }
                public string email { get; set; }
                public string id_media { get; set; }
                public int pagesize { get; set; } = 8;
                public int limit { get; set; } = 8;
                public int page { get; set; } = 8;
                public string min_harga { get; set; }
                public string max_harga { get; set; }
                public int max_media { get; set; } = 2;
                public string jenis_media { get; set; }
                public string orderBy { get; set; }
                public string orderDir { get; set; }
                public string id_kategori { get; set; }
                public string kategoriid_list { get; set; }
                public string id_brand { get; set; }
                public string brandid_list { get; set; }
                public string noid_list { get; set; }
                public bool strict_kategori { get; set; } = true;
                public bool all_item { get; set; }
                public string slug { get; set; }
                public bool show_variant { get; set; } = true;
                public string cursor_id { get; set; }
                public string cursor_nama { get; set; }
                public string cursor_harga { get; set; }
                public string cursor_no_bukti { get; set; }
                public string id_customer { get; set; }
                public string id_company { get; set; }
                public int produk_limit { get; set; }
                public string no_tracking_pesanan { get; set; }
                public string id_cart { get; set; }
                public string status_bayar { get; set; }
                public string id_payment_gateway { get; set; }
                public string payment_type { get; set; }
                public string id_detil { get; set; }
                public string show_media { get; set; }
                public string no_bukti { get; set; }
                public string status_order_1 { get; set; }
                public string status_order_2 { get; set; }
                public string status_item { get; set; }
                public string status { get; set; }
                public string keyword { get; set; }
                public string kode { get; set; }
            }
        }
}