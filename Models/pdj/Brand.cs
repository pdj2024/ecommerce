using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
 
    public partial class Brand
    {
        public string No_Id { get; set; } = "";
        public string Nama_Brand { get; set; } = "";
        public string Nama_Banner { get; set; } = "";
        public string Status { get; set; } = "aktif";
        public string Id_Media { get; set; } = "";
        public string No_Id_Media { get; set; } = "";

    }
}