using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class LoginUserVM
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email no formato incorreto")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha obrigatório")]
        public string Password { get; set; }
    }
}
