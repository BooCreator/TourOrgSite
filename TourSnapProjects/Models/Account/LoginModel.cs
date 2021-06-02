using System;
using System.ComponentModel.DataAnnotations;

namespace TourSnapProjects.Models.Account
{
    /// <summary>
    /// Модель для входа в систему
    /// </summary>
    public class LoginModel
    {
        [Required]
        public String MailOrPhone { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }
    }
}