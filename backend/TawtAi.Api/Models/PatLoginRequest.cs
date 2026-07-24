using System.ComponentModel.DataAnnotations;

namespace TawtAi.Api.Models;

public class PatLoginRequest
{
    [Required(ErrorMessage = "Personal Access Token is required.")]
    public string PatToken { get; set; } = string.Empty;
}
