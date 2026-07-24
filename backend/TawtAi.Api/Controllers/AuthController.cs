using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TawtAi.Api.Models;
using TawtAi.Api.Services;

namespace TawtAi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(AuthAppService authAppService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(PatLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PatLoginResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatLoginResponse>> Login([FromBody] PatLoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authAppService.LoginAsync(request, cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
