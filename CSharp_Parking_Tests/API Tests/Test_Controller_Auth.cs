using CSharpAPI.Controllers;
using CSharpAPI.Database;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Security.Claims;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Controller_Auth
    {
        [Fact]
        public void Me_With_Null_User_Identity_Should_Handle_Gracefully()
        {
            // Arrange - create controller with mocked HttpContext
            var mockDb = new Mock<SQLite_Database>();
            var mockTokenService = new Mock<ITokenService>();
            var mockTokenRevocationService = new Mock<ITokenRevocationService>();
            var mockLogger = new Mock<ILogger<C_Auth>>();

            var controller = new C_Auth(mockDb.Object, mockTokenService.Object, mockTokenRevocationService.Object);

            // Mock HttpContext with null User.Identity
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Empty identity, no claims
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Me();

            // Assert - should handle null gracefully
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
        }

        [Fact]
        public void Me_With_Missing_Claims_Should_Handle_Gracefully()
        {
            // Arrange
            var mockDb = new Mock<SQLite_Database>();
            var mockTokenService = new Mock<ITokenService>();
            var mockTokenRevocationService = new Mock<ITokenRevocationService>();
            var mockLogger = new Mock<ILogger<C_Auth>>();

            var controller = new C_Auth(mockDb.Object, mockTokenService.Object, mockTokenRevocationService.Object);

            // Mock HttpContext with User.Identity but missing claims
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                // Missing NameIdentifier and Role claims
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Me();

            // Assert - should handle missing claims gracefully
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
        }
    }
}