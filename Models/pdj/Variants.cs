using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
 
    public partial class Variants
    {
        public string No_Id { get; set; } = "";
        public string Id_Produk { get; set; } = "";
        public string Size { get; set; } = "";
        public int Harga { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public string Sku { get; set; } = "";
        public string Status { get; set; } = "";
        public string Option { get; set; } = "";
    }
}