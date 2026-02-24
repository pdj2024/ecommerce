using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebShared.Models
{

    public partial class DataLogin
    {
        public string No_Id { get; set; } = "";
        public string Nama_Display { get; set; } = "";
        public string Telepon { get; set; } = "";
        public string sessionId { get; set; } = "";
        public string tokenJwt { get; set; } = "";
        public string Password { get; set; } = "";
        public string token { get; set; } = "";
        public string refreshToken { get; set; } = "";
        public string XIDivice { get; set; } = "";
        public bool StrichLogin { get; set; }
        public DataUser DataUser { get; set; } = new();
        public string? TanggalRefresh { get; set; } = null;
    }

}