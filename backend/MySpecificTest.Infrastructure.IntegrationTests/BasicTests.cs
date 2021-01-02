using System.Threading.Tasks;
using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests
{
    public class BasicTests
        : IClassFixture<CustomWebApplicationFactory<MySpecificTest.WebApi.Startup>>
    {
        private readonly CustomWebApplicationFactory<MySpecificTest.WebApi.Startup> _factory;

        public BasicTests(CustomWebApplicationFactory<MySpecificTest.WebApi.Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/WeatherForecast")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
    }
}