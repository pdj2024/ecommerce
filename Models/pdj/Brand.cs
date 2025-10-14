using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
 
    public partial class Brand
    {
        public string Id { get; set; } = "";
        public string No_Id { get; set; } = "";
        public string Nama_Brand { get; set; } = "";
        public string Nama_Banner { get; set; } = "";
        public string Status { get; set; } = "aktif";
        public string Id_Media_Desktop { get; set; } = "";
        public string Id_Media_Tablet { get; set; } = "";
        public string Id_Media_Phone { get; set; } = "";
        public MediaData Media_Desktop { get; set; } = new();
        public MediaData Media_Tablet { get; set; } = new();
        public MediaData Media_Phone { get; set; } = new();
        public DateTime Tgl_Start { get; set; } = new();
        public DateTime Tgl_Finish { get; set; } = new();
        public string Id_Media { get; set; } = "";
        public string No_Id_Media { get; set; } = "";
        public bool Checked_Filter { get; set; } = false;
    }
}