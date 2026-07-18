using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TawtAi.Api.Tests;

public class WeatherForecastEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_WeatherForecast_ReturnsOk()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
