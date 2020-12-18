using EtkBlazorApp.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.ViewModel
{
    public class ShopAccountViewModel
    {
        public event Action ProgressBarStateChanged;

        [Url(ErrorMessage = "Введите валидный URL")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string Uri { get; set; }

        [MinLength(4, ErrorMessage = "Минимальная длина 4 символа")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string Title { get; set; }

        public int Id { get; set; }

        public bool IsSelected { get; set; }

        public bool IsUpdating { get; set; }

        public string Favicon => !string.IsNullOrWhiteSpace(Uri) ? $"{Uri}/favicon.ico" : string.Empty;

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string FTP_Host { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string FTP_Login { get; set; }

        [MinLength(8, ErrorMessage = "Слишком простой пароль (минимальная длина 8 символов)")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string FTP_Password { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string DB_Host { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string DB_Login { get; set; }

        [MinLength(8, ErrorMessage = "Слишком простой пароль (минимальная длина 8 символов)")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string DB_Password { get; set; }

        public void ActivateProgressBar()
        {
            IsUpdating = true;
            ProgressBarStateChanged?.Invoke();
        }

        public void DeactivateProgressBar()
        {
            IsUpdating = false;
            ProgressBarStateChanged?.Invoke();
        }

    }
}
