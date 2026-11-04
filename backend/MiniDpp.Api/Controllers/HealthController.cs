using Microsoft.AspNetCore.Mvc;
using MiniDpp.Api.DTOs;

namespace MiniDpp.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("Healthy", "MiniDpp.Api"));
    }
}
