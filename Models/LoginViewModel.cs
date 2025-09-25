using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email wajib diisi")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi")]
        public string? Password { get; set; }
    }
}
