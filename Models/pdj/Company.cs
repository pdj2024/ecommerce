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


        public Company DataCompany { get; set; } = new Company();
        public Produk DataProduk { get; set; } = new Produk();
        public Cart DataCart { get; set; } = new Cart();
        public Brand DataBanner { get; set; } = new Brand();
        public DataUser DataUser { get; set; } = new DataUser();
        public Brand DataBrand { get; set; } = new Brand();
        public Pesanan DataPesanan { get; set; } = new Pesanan();
        public Navbar DataNavbar { get; set; } = new Navbar();
        public Pembayaran DataPembayaran { get; set; } = new Pembayaran();
        public Pengiriman DataPengiriman { get; set; } = new Pengiriman();


        public (List<Brand> SearchBrand, List<Produk> SearchProduk, int TotalCount, bool hasMore) SearchList { get; set; } = (new(), new(), 0, false);

        public bool Finised { get; set; } = false;
    }

    public partial class Company
    {
        public string No_Id { get; set; } = "";
        public string Nama_Toko { get; set; } = "";
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
