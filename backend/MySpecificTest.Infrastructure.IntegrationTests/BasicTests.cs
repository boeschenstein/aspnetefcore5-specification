using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static MySpecificTest.WebApi.Controllers.BlogController;

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

        [Fact]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType_WeatherForecast()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/WeatherForecast");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType_doesnotexist()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/doesnotexist");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_EndpointsReturnSuccessAndCorrectContentType_Blog()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var content = Serialize(new UrlRequestDto { Url = "my.test.blog" });
            var response = await client.PostAsync("/Blog", new StringContent(content, Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            string result = await response.Content.ReadAsStringAsync();
            var res = Deserialize<IEnumerable<Blog>>(result); // case-sensitive
            //var res = DeserializeNewtonsoft<IEnumerable<Blog>>(result); // NewtonSoft

            var blog = res.First();
            Assert.Equal(-1, blog.BlogId);
            Assert.Equal("my.test.blog", blog.Url);

            // FluentAssertions: https://fluentassertions.com/introduction
            blog.BlogId.Should().Be(-1);
            blog.Url.Should().StartWith("my").And.EndWith("blog").And.Contain("test").And.HaveLength(12);
            blog.Should().BeEquivalentTo(new Blog { BlogId = -1, Url = "my.test.blog" });
        }

        [Fact]
        public async Task Put_EndpointsReturnSuccessAndCorrectContentType_Blog()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var content = Serialize(new UrlRequestDto { Url = "my.test.blog" });
            var response = await client.PutAsync("/Blog", new StringContent(content, Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

            string result = await response.Content.ReadAsStringAsync();
            var res = Deserialize<IEnumerable<Blog>>(result);
            //var res = DeserializeNewtonsoft<IEnumerable<Blog>>(result); // NewtonSoft

            var blog = res.First();
            Assert.Equal(-1, blog.BlogId);
            Assert.Equal("my.test.blog", blog.Url);
        }

        private string Serialize<T>(T value)
        {
            // https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/
            return System.Text.Json.JsonSerializer.Serialize<T>(value);
        }

        private T Deserialize<T>(string value)
        {
            // https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/

            var serializeOptions = new System.Text.Json.JsonSerializerOptions
            {
                // case-sensitive per default. To change it, set this:
                PropertyNameCaseInsensitive = true, // similar behavior to newtonsoft
            };
            return System.Text.Json.JsonSerializer.Deserialize<T>(value, serializeOptions);
        }

        private T SerializeNewtonsoft<T>(string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }

        private T DeserializeNewtonsoft<T>(string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }
    }
}