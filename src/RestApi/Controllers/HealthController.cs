using Microsoft.AspNetCore.Mvc;

namespace RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthcheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "rest-api/healthcheck OK" });
}
