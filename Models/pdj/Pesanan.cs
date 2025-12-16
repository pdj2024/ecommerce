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
            public string No_Tracking { get; set; } = "";
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
            public bool variabel_map_1 { get; set; } = false;
            public bool variabel_map_2 { get; set; } = false;
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
            public string Var_Size_Label { get; set; } = "";
            public decimal Var_Harga { get; set; } = 0;
            public int Var_Stock { get; set; } = 0;
            public string Var_Sku { get; set; } = "";
            public string Var_Id_Media { get; set; } = "";
            public string Var_Status { get; set; } = "";
            public string Var_Option { get; set; } = "";
            public string Var_Option_Label { get; set; } = "";
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


        public class BuatOrder
        {
            public int StatusCode { get; set; } = 0;
            public string Message { get; set; } = "";
            public string NewId { get; set; } = "";
        }

        public class Datas
        {
            public bool Payment_Problem { get; set; } = false;
            public List<Fail_Gen_Payments> Fail_Gen_Payment { get; set; } = new();
            public string[] Success_Gen_Payment { get; set; } = [];
            public List<Headers> Header { get; set; } = new();
            public List<Item> Items { get; set; } = new();
            public List<Biayas> Biaya { get; set; } = new();
        }

        public class Fail_Gen_Payments
        {
            public string No_Bukti { get; set; } = "";
            public string Error { get; set; } = "";
        }

        public class PaymentMidtrans
        {
            public int StatusCode { get; set; } = 0;
            public string Message { get; set; } = "";
            public MidtransResponses MidtransResponse { get; set; } = new();
        }

        public class ActionsMidtrans
        {
            public string name { get; set; } = "";
            public string method { get; set; } = "";
            public string url { get; set; } = "";
        }

        public class UpdateOrder
        {
            public int StatusCode { get; set; } = 0;
            public string Message { get; set; } = "";
        }

        public class SummaryInfo
        {
            public int jumlah { get; set; } = 0;
            public string status_order_1 { get; set; } = "";
        }


        public class PaymentResponse
        {
            public BuatOrder BuatPesanan { get; set; } = new();
            public PaymentMidtrans Payment { get; set; } = new();
            public UpdateOrder UpdatePesanan { get; set; } = new();
        }

        public class SummaryResponse
        {
            public string id_customer { get; set; } = "";
            public int total_pesanan { get; set; } = 0;
            public List<SummaryInfo> summary { get; set; } = new();

        }

        public class MidtransResponses
        {
            public string status_code { get; set; } = "";
            public string status_message { get; set; } = "";
            public string transaction_id { get; set; } = "";
            public string order_id { get; set; } = "";
            public string merchant_id { get; set; } = "";
            public string gross_amount { get; set; } = "";
            public string currency { get; set; } = "";
            public string payment_type { get; set; } = "";
            public string transaction_time { get; set; } = "";
            public string transaction_status { get; set; } = "";
            public string fraud_status { get; set; } = "";
            public string expiry_time { get; set; } = "";
            public string qr_string { get; set; } = "";
            public List<ActionsMidtrans> actions { get; set; } = new();

        }



        public int StatusCode { get; set; } = 0;
        public string Message { get; set; } = "";
        public int TotalCount { get; set; } = 0;
        public int ReturnedCount { get; set; } = 0;
        public int RemainingCount { get; set; } = 0;
        public string LastCursor { get; set; } = "";
        public bool hasMore { get; set; } = true;
        public Datas Data { get; set; } = new();

    }

}
