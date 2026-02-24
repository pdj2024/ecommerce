using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
 
    public partial class CurrentUserLogin
    {
        public static string software_name { get; set; }
public static string nama_company_global { get; set; }
public static string sub_title_global { get; set; }

public static string nama_user_global { get; set; }
public static string user_level { get; set; }
public static int id_user_global { get; set; }
public static List<string> list_menu_access_user_global = new List<string> { };
public static List<string> node_menu_access_user_global = new List<string> { };

public static bool bisa_lihat_nilai { get; set; }
public static bool bisa_simpan_voucher { get; set; }

           }
}
