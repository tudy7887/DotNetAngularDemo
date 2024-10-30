using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class LoginDto
{
    [Required]
    public required string Username { get; set; }
    [Required]
    [StringLength(8, MinimumLength = 4)]
    public required string Password { get; set; }

}
