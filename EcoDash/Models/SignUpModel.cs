﻿using System.ComponentModel.DataAnnotations;

namespace EcoDash.Models
{
    public class SignUpModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
