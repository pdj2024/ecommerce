using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
 
    public partial class ParameterApi
    {
        public string defaultUrl { get; set; } = "http://43.228.212.41:77/api/";
        public string routeApi { get; set; } = "";
        public string prosesApi { get; set; } = "";
        public ParameterUrl ParamUrl { get; set; } 


        public class ParameterUrl
        {
            public string no_id { get; set; }
            public string id_media { get; set; }
            public int pagesize { get; set; } = 8;
            public string min_harga { get; set; }
            public string max_harga { get; set; }
            public string jenis_media { get; set; }
            public string orderBy { get; set; }
            public string orderDir { get; set; }
            public string id_kategori { get; set; }
            public string slug { get; set; }
            public bool show_variant { get; set; } = true;
            public string cursor_id { get; set; }
            public string cursor_nama { get; set; }
            public string cursor_harga { get; set; }
            public string id_customer { get; set; }
            public string id_cart { get; set; }
            public string status_bayar { get; set; }
            public string id_payment_gateway { get; set; }
            public string id_detil { get; set; }
            public string show_media { get; set; }
            public string no_bukti { get; set; }
            public string status_order_1 { get; set; }
            public string status_order_2 { get; set; }
            public string status_item { get; set; }
            public string status { get; set; }
            public string keyword { get; set; }
        }
    
    }



}