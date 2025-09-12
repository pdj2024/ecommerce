using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public partial class Pesanan
    {
        public class Headers
        {
            public string No_Bukti { get; set; } = "";
            public string Status_Order_1 { get; set; } = "";
            public string Status_Order_2 { get; set; } = "";
            public string Id_Customer { get; set; } = "";
            public string Status_Bayar { get; set; } = "";
            public string Id_Payment_Gateway { get; set; } = "";
            public string Alamat_Kirim { get; set; } = "";
            public string Metode_Bayar { get; set; } = "";
            public string Payment_Type { get; set; } = "";
            public DateTime? Expiry_Time { get; set; }
            public string? Payment_Number_1 { get; set; } = "";
            public string? Payment_Number_2 { get; set; } = "";
            public DateTime Created_At { get; set; } = new();
            public DateTime Updated_At { get; set; } = new();
            public string Order_Id_Midtrans { get; set; } = "";
            public string Nama_Penerima { get; set; } = "";
            public string Telpon_Penerima { get; set; } = "";
            public string Id_Shipping_Option { get; set; } = "";
            public string Nama_Shipping_Option { get; set; } = "";
            public decimal Subtotal { get; set; } = 0;
            public decimal Tot_DPP { get; set; } = 0;
            public decimal PPN { get; set; } = 0;
            public bool Incl_PPN { get; set; } = true;
            public decimal Tot_PPN { get; set; } = 0;
            public decimal Grand_Total { get; set; } = 0;
            public string QrBase64 { get; set; } = "";
        }
        
        public class Item
        {
            public int Id_Detil { get; set; } = 0;
            public string No_Bukti { get; set; } = "";
            public string Jenis_Item { get; set; } = "";
            public string Id_Variant { get; set; } = "";
            public string Nama_Display { get; set; } = "";
            public int Qty { get; set; } = 0;
            public decimal Harga_Satuan { get; set; } = 0;
            public decimal Harga_Total { get; set; } = 0;
            public string Status_Item { get; set; } = "";
            public string Var_Id_Produk { get; set; } = "";
            public string Var_Size { get; set; } = "";
            public decimal Var_Harga { get; set; } = 0;
            public int Var_Stock { get; set; } = 0;
            public string Var_Sku { get; set; } = "";
            public string Var_Id_Media { get; set; } = "";
            public string Var_Status { get; set; } = "";
            public string Var_Option { get; set; } = "";
        }

        public class Biayas
        {
            public int Id_Detil { get; set; } = 0;
            public string No_Bukti { get; set; } = "";
            public string Jenis_Item { get; set; } = "";
            public string Id_Variant { get; set; } = "";
            public int Qty { get; set; } = 0;
            public decimal Harga_Satuan { get; set; } = 0;
            public decimal Harga_Total { get; set; } = 0;
        }

        public int HeaderRows { get; set; } = 0;
        public int ItemRows { get; set; } = 0;
        public int BiayaRows { get; set; } = 0;
        public List<Headers> Header { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public List<Biayas> Biaya { get; set; } = new();




    }

}
