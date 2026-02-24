using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
    public partial class DataUser
    {
        public string No_Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Nama_Display { get; set; } = "";
        public string Telepon { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Kota { get; set; } = "";
        public string Status { get; set; } = "";
        public bool Pernah_Order { get; set; } = false;
        public string Id_Media { get; set; } = "";
        public string Alamat { get; set; } = "";

        //public DateTime Tgl_Created { get; set; } = new();
        //public DateTime? Tgl_NonAktif { get; set; } = new();
    }

    public class FeaturAllow
    {
        public bool GmapShipping { get; set; } = false;
        public bool CartLogin { get; set; } = false;
        public bool StockActive { get; set; } = false;
        public bool PaymentLogin { get; set; } = false;
    }
}
