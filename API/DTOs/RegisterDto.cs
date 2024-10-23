using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    [MinLength(4)]
    public required string Username { get; set; }

    [Required]
    [MaxLength(100)]
    [MinLength(8)]
    public required string Password { get; set; }
}
