using System.ComponentModel.DataAnnotations;

namespace CakeyNuts.Api.Dtos;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(10)]
    public string Password { get; set; } = "";
}
