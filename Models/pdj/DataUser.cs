using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public partial class DataUser
    {
      public string versi_database { get; set; } = "";
public string user_level { get; set; }
public string node1 { get; set; }
public string node2 { get; set; }
public string node3 { get; set; }
public string user_name { get; set; }

public string folder_backup { get; set; }
public string folder_manual_backup { get; set; }
public int frekuensi { get; set; }
public string user_pass { get; set; }
public bool aktif { get; set; }
public bool bisa_lihat_nilai { get; set; } = false;
public bool bisa_simpan_voucher { get; set; } = false;
public string keterangan { get; set; }
public string deleted { get; set; }
public int no_id { get; set; }
public int user_id { get; set; }
public int life { get; set; } = -99;

public int jumlah_login { get; set; } = 9999;

public int cloning_source { get; set; }

public string menu_name { get; set; }
public string menu_tipe { get; set; }
public string no_menu { get; set; }

public bool can_create { get; set; }
public bool can_update { get; set; }
public bool can_read { get; set; }
public bool can_delete { get; set; }
public bool can_print { get; set; }

public string no_bukti { get; set; }
public string jenis_transaksi { get; set; }
public string proses { get; set; }

public string keterangan2 { get; set; }
public string keterangan3 { get; set; }
public DateTime tgl_pakai { get; set; }
public string nama_user { get; set; }
public string head_atau_det { get; set; }
public int id_det { get; set; }


public string serial { get; set; }
public string disk { get; set; }
public string nama { get; set; }



    }
}
