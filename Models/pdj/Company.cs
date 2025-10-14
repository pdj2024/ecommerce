using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Navbar
    {
        public int Priority { get; set; } = 0;
        public string Nav_Type { get; set; } = "";
        public string Display_Text { get; set; } = "";
        public string Id_Company { get; set; } = "";
        public List<string> Filter { get; set; } = new();
        public List<string> Id_Brand { get; set; } = new();
        public List<Brand> Brand { get; set; } = new();
        public List<string> Id_Kategori { get; set; } = new();
        public List<Kategori> Kategori { get; set; } = new();
        public List<Produk> Produk { get; set; } = new();
    }

    public class CompanySosial
    {
        public string Id_Company { get; set; } = "";
        public string Social_Type { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Url { get; set; } = "";
        public string Media_Type { get; set; } = "";
        public string Id_Media { get; set; } = "";
        public string No_Id_Media { get; set; } = "";
    }

    public class NilaiDefault
    {
        public int Limit_Produk_Home { get; set; } = 12;
        public int Limit_Produk_Search { get; set; } = 12;
        public decimal Ppn { get; set; } = 0;
        public decimal Expiry_In_Day { get; set; } = 0;
        public decimal Expiry_In_Hour { get; set; } = 0;
        public decimal Expiry_In_Minute { get; set; } = 0;
        public bool Incl_Ppn { get; set; } = false;
        public string Id_Company { get; set; } = "";
    }

    public class SourchData
    {
        public List<Company> ListDataCompany { get; set; } = new List<Company>();
        public List<Brand> ListDataBanner { get; set; } = new List<Brand>();
        public List<Brand> ListDataBrand { get; set; } = new List<Brand>();
        public List<Produk> ListDataProduk { get; set; } = new List<Produk>();
        public List<Kategori> ListDataKategori { get; set; } = new List<Kategori>();
        public List<Cart> ListDataCart { get; set; } = new List<Cart>();
        public List<Navbar> ListDataNavbar { get; set; } = new List<Navbar>();
        public List<DataUser> ListDataUser { get; set; } = new List<DataUser>();
        public List<Pembayaran> ListDataPembayaran { get; set; } = new List<Pembayaran>();
        public List<Pengiriman> ListDataPengiriman { get; set; } = new List<Pengiriman>();
        public List<NilaiDefault> ListDataNilaiDefault { get; set; } = new List<NilaiDefault>();
        public List<CompanySosial> ListDataCompanySocial { get; set; } = new List<CompanySosial>();
        public List<Pesanan.SummaryResponse> ListDataSummaryPesanan { get; set; } = new List<Pesanan.SummaryResponse>();




        public NilaiDefault DataNilaiDefault { get; set; } = new NilaiDefault();
        public Company DataCompany { get; set; } = new Company();
        public CompanySosial DataCompanySocial { get; set; } = new CompanySosial();
        public Produk DataProduk { get; set; } = new Produk();
        public Cart DataCart { get; set; } = new Cart();
        public Brand DataBanner { get; set; } = new Brand();
        public DataUser DataUser { get; set; } = new DataUser();
        public Brand DataBrand { get; set; } = new Brand();
        public Pesanan DataPesanan { get; set; } = new Pesanan();
        public Pesanan DataPesananBelum { get; set; } = new Pesanan();
        public Pesanan DataPesananDikemas { get; set; } = new Pesanan();
        public Pesanan DataPesananDikirim { get; set; } = new Pesanan();
        public Pesanan DataPesananHistori { get; set; } = new Pesanan();
        public Navbar DataNavbar { get; set; } = new Navbar();
        public Pembayaran DataPembayaran { get; set; } = new Pembayaran();
        public Pengiriman DataPengiriman { get; set; } = new Pengiriman();
        public Pesanan.PaymentResponse DataResponsePesanan { get; set; } = new Pesanan.PaymentResponse();


        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new(), new(), 0, false);

        public bool Finised { get; set; } = false;
    }

    public partial class Company
    {
        public string No_Id { get; set; } = "";
        public string Nama_Toko { get; set; } = "Hallow All";
        public string Email { get; set; } = "";
        public string No_Telp { get; set; } = "";
        public string No_Wa_1 { get; set; } = "";
        public string No_Wa_2 { get; set; } = "";
        public string Alamat_Toko { get; set; } = "";
        public string Hari_Buka { get; set; } = "";
        public string Jam_Buka { get; set; } = "";
        public string Kode_Pos { get; set; } = "";
        public string About { get; set; } = "";
        public string Gmap { get; set; } = "";
        public List<CompanySosial> Media { get; set; } = new();
    }

}
