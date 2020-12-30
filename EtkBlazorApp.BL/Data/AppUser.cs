using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp.BL.Data
{
    public class AppUser
    {
        [Required(ErrorMessage = "Поле логин обязательно для заполнения")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Поле пароль обязательно для заполнения")]
        [StringLength(maximumLength: 16, MinimumLength = 6, ErrorMessage = "Минимальная длина пароля 6 символов")]
        public string Password { get; set; }

        public int InvalidPasswordCounter { get; set; }
    }
}
