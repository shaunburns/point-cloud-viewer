using Microsoft.EntityFrameworkCore;
using PointCloudViewer.Server;
using PointCloudViewer.Server.DTOs;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;

namespace PointCloudViewer.ServerTests.IntegrationTests
{
    public class AuthTests : IClassFixture<TestsWebApplicationFactory<Program>>
    {
        private readonly TestsWebApplicationFactory<Program> _factory;

        public AuthTests(TestsWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("admin", "admin", HttpStatusCode.OK)]
        [InlineData("admin", "wrong", HttpStatusCode.Unauthorized)]
        [InlineData("wrong", "admin", HttpStatusCode.Unauthorized)]
        [InlineData("wrong", "wrong", HttpStatusCode.Unauthorized)]
        public async Task Login_WithCredentials_ReturnsExpectedResult(string username, string password, HttpStatusCode expected)
        {
            // Arrange
            var client = _factory.CreateClient();
            var requestContent = new UserLoginRequestDTO
            {
                Username = username,
                Password = password,
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", requestContent, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(expected, response.StatusCode);
        }
    }
}
