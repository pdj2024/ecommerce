using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public partial class ParamUrl
    {
        public string No_Id { get; set; } = "";
        public string Id_Kategori { get; set; } = "";
        public string Nama_Display { get; set; } = "";
        public DateTime Tgl_Created { get; set; }
        public decimal Harga { get; set; } = 0;
        public decimal? Diskon1 { get; set; } = 0;
        public decimal? Diskon2 { get; set; } = 0;
        public decimal? Diskon3 { get; set; } = 0;
        public decimal? Diskon4 { get; set; } = 0;
        public string Satuan { get; set; } = "";
        public int Min_Order { get; set; } = 0;
        public string Deskripsi { get; set; } = "";
        public List<string> Size { get; set; } = new();
        public string Size_Default { get; set; } = "";
        public bool Special_Price { get; set; } = false;
        public string Kode_Erp { get; set; } = "";
        public List<MediaData> MediaUrls { get; set; } = new List<MediaData>();
    }
}
