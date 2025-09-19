using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
 
    public partial class Kategori
    {
        public string No_Id { get; set; } = "";
        public string Nama_Kategori { get; set; } = "";
        public string Nama_Group_Kategori { get; set; } = "";
        public List<string> Group_Kategori { get; set; } = new List<string>();
        public string Status { get; set; } = "aktif";
        public bool Checked_Filter { get; set; } = false;
        public string Id_Media { get; set; } = "";
        public string No_Id_Media { get; set; } = "";




    }
}