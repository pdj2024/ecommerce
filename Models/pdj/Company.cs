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
        public string Id_Media { get; set; } = "";
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
    }

}
