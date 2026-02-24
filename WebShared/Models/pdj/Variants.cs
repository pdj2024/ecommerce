using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
 
    public partial class Variants
    {
        public string No_Id { get; set; } = "";
        public string Id_Produk { get; set; } = "";
        public string Size_Id { get; set; } = "";
        public string Size_Label { get; set; } = "";
        public int Harga { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public string Sku { get; set; } = "";
        public string Status { get; set; } = "";
        public string Option_Id { get; set; } = "";
        public string Option_Label { get; set; } = "";
    }
}