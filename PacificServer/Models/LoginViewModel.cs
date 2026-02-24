using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nomor Handphone wajib diisi")]
        public string? NoTlp { get; set; }

        [Required(ErrorMessage = "Password wajib diisi")]
        public string? Password { get; set; }

        public string Jenis_User { get; set; } = "customer";
    }
}
