using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
 
    public partial class MasterCustomer
    {
       public int user_id { get; set; }
public string no_cussup { get; set; } = "";
public string nm_cussup { get; set; } = "";
public string owner { get; set; } = "";
public string alamat { get; set; } = "";

public string alamat1 { get; set; } = "";
public string alamat2 { get; set; } = "";
public string alamat3 { get; set; } = "";
public string pilihan_alamat { get; set; } = "";
public string no_alamat_kirim { get; set; } = "";
public string alamat_kirim { get; set; } = "";
public string alamat_kirim1 { get; set; } = "";

public string alamat_kirim2 { get; set; } = "";
public string alamat_kirim3 { get; set; } = "";
public string kelurahan { get; set; } = "";
public string kecamatan { get; set; } = "";
public string kota { get; set; } = "";
public string kodepos { get; set; } = "";
public string telpon { get; set; } = "";
public string email { get; set; } = "";
public string kontak { get; set; } = "";
public string no_npwp { get; set; } = "";
public string nama_npwp { get; set; } = "";
public string alamat_npwp { get; set; } = "";
public string kota_npwp { get; set; } = "";
public decimal saldo_awal { get; set; } = 0;
public decimal plafon { get; set; } = 0;
public int tempo { get; set; } = 0;
public decimal saldo_akhir { get; set; } = 0;
public bool aktif { get; set; } = true;
        public string aktif_text { get; set; } = "";
public string keterangan { get; set; } = "";
public DateTime tgl_jam { get; set; }
public string user { get; set; }

public decimal omzet { get; set; } = 0;



    }
}