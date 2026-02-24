using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
    public partial class Pembayaran
    {
        public string Cara_Bayar { get; set; } = "";
        public string Display { get; set; } = "";
        public string Deskripsi { get; set; } = "";
        public string Provider { get; set; } = "";
        public int Fee { get; set; } = 0;
        public string Status { get; set; } = "";
        public bool selectPayment { get; set; } = false;

        public bool disabeldPayment { get; set; } = false;
    }
}
