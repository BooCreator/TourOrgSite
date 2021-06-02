using System;
using System.ComponentModel.DataAnnotations;

namespace TourSnapProjects.Models.Account
{
    /// <summary>
    /// Модель для регистрации
    /// </summary>
    public class RegisterModel
    {
        [Required]
        public String Name { get; set; }
        [Required]
        public String Mail { get; set; }
        [Required]
        public Int32 Phone { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public String ConfirmPassword { get; set; }
    }

    
}