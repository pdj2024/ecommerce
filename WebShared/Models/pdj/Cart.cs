using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
 
    public partial class Cart
    {
        public string No_Id { get; set; } = "";
        public string Status { get; set; } = "aktif";
        public string Id_Media { get; set; } = "";
        public string Id_Cart { get; set; } = "";
        public string Id_Customer { get; set; } = "";
        public string Id_Variant { get; set; } = "";
        public string Id_Produk { get; set; } = "";
        public string Nama_Display { get; set; } = "";
        public int Qty { get; set; } = 0;
        public decimal? Diskon1 { get; set; } = 0;
        public decimal? Diskon2 { get; set; } = 0;
        public decimal? Diskon3 { get; set; } = 0;
        public decimal? Diskon4 { get; set; } = 0;
        public int add_qty { get; set; } = 0;
        public string Created_At { get; set; } = "";
        public string Size_Id { get; set; } = "";
        public string Size_Label { get; set; } = "";
        public string Sku { get; set; } = "";
        public string Option_Id { get; set; } = "";
        public string Option_Label { get; set; } = "";
        public string Status_Variant { get; set; } = "";
        public string Status_Produk { get; set; } = "";
        public string Jenis_Item { get; set; } = "";
        public bool Show_Min_Order { get; set; } = false;
        public bool Show_Max_Order { get; set; } = false;
        public bool Show_Add_Product { get; set; } = false;
        public decimal? Harga { get; set; } = 0;
        public decimal Harga_Satuan { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public int Min_Order { get; set; } = 0;
        public bool SelectCart { get; set; } = true;

    }
}