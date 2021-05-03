using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class AppUser 
    {
        public int Id { get; set; }

        public bool IsEnabled { get; set; }

        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")]
        public string AllowedIp { get; set; }

        [Required]
        [StringLength(maximumLength: 16, MinimumLength = 6, ErrorMessage = "Поле логин обязательно для заполнения (2 - 32 символа)")]
        public string Login { get; set; }

        [Required]
        [StringLength(maximumLength: 16, MinimumLength = 6, ErrorMessage = "Необходимо заполнить пароль (6 - 16 символов)")]
        public string Password { get; set; }

        public string UserIP { get; set; }

        public string GroupName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Необходимо выбрать группу пользователя")]
        public int GroupId{ get; set; }

        public DateTime CreatingDate { get; set; }

        public DateTime? LastLoginDateTime { get; set; }

        public bool PasswordUpdated { get; set; }

        bool? hasAllowedIp = null;
        public bool HasAllowedIp 
        { 
            get => hasAllowedIp ?? !string.IsNullOrWhiteSpace(AllowedIp); 
            set => hasAllowedIp = value; 
        }
    }
}
