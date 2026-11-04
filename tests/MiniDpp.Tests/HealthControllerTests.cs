using Microsoft.AspNetCore.Mvc;
using MiniDpp.Api.Controllers;
using MiniDpp.Api.DTOs;

namespace MiniDpp.Tests;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        var controller = new HealthController();

        var result = controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.Equal("Healthy", response.Status);
        Assert.Equal("MiniDpp.Api", response.Service);
    }
}
