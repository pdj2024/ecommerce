using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Options
    {
        public string Option_Id { get; set; } = "";
        public string Option_Label { get; set; } = "";
        public string Id_Media { get; set; } = "";
        public MediaData MediaUrl { get; set; } = new MediaData();
    }

    public class Size
    {
        public string Size_Id { get; set; } = "";
        public string Size_Label { get; set; } = "";
    }

    public partial class Produk
    {
        public string No_Id { get; set; } = "";
        public List<string> Id_Kategori { get; set; } = new();
        public List<string> Nama_Kategori { get; set; } = new();
        public string Id_Brand { get; set; } = "";
        public string Id_Variant { get; set; } = "";
        public string Nama_Brand { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Nama_Display { get; set; } = "";
        public DateTime Tgl_Created { get; set; } = new DateTime();
        public decimal Harga { get; set; } = 0;
        public decimal? Diskon1 { get; set; } = 0;
        public decimal? Diskon2 { get; set; } = 0;
        public decimal? Diskon3 { get; set; } = 0;
        public decimal? Diskon4 { get; set; } = 0;
        public string Satuan { get; set; } = "";
        public int Min_Order { get; set; } = 0;
        public bool Show_Min_Order { get; set; } = false;
        public int Order_Qty { get; set; } = 0;
        public string Deskripsi { get; set; } = "";
        public List<Size> Sizes { get; set; } = new();
        public List<string> Id_Media { get; set; } = new();
        public string Size_Default { get; set; } = "";
        public string Id_Size_Default { get; set; } = "";
        public bool Special_Price { get; set; } = false;
        public string Kode_Erp { get; set; } = "";
        public string Media { get; set; } = "";
        public string Option_Default { get; set; } = "";
        public string Id_Option_Default { get; set; } = "";
        public List<MediaData> MediaUrls { get; set; } = new();
        public List<Options> Options { get; set; } = new();
        public List<Variants> Variants { get; set; } = new();
    }
}
