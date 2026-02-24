using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
    public partial class Pengiriman
    {
        public string No_Id { get; set; } = "";
        public string Nama { get; set; } = "";
        public int Fee { get; set; } = 0;
        public string Status { get; set; } = "";
        public bool selectShipping { get; set; } = false;
    }
}
