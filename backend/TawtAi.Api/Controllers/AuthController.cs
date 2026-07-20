using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TawtAi.Api.Models;
using TawtAi.Api.Services;

namespace TawtAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _authAppService;

    public AuthController(AuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<PatLoginResponse>> Login([FromBody] PatLoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authAppService.LoginAsync(request, cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
