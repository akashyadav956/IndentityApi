using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Models.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username cannot be empty")]
        [MinLength(6, ErrorMessage = "Minimum 6 character required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(8, ErrorMessage = "Minimum 8 character required")]
        public string Password { get; set; }
    }
}
