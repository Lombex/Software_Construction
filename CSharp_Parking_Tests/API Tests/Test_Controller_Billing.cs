using CSharpAPI.Controllers;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Security.Claims;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Controller_Billing
    {
        [Fact]
        public async Task GetBills_With_Missing_UserId_Claim_Should_Return_Unauthorized()
        {
            // Arrange
            var mockBillingService = new Mock<IBillingService>();

            var controller = new C_Billing(mockBillingService.Object);

            // Mock HttpContext with User.Identity but missing NameIdentifier claim
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "ParkingUser"),
                // Missing NameIdentifier claim
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
            var result = await controller.GetMine();

            // Assert - should return Unauthorized due to missing User ID claim
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetMine_With_Invalid_UserId_Claim_Should_Return_Unauthorized()
        {
            // Arrange
            var mockBillingService = new Mock<IBillingService>();

            var controller = new C_Billing(mockBillingService.Object);

            // Mock HttpContext with invalid NameIdentifier claim (not a GUID)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "not-a-guid"),
                new Claim(ClaimTypes.Role, "ParkingUser"),
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
            var result = await controller.GetMine();

            // Assert - should return Unauthorized due to invalid User ID claim
            result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}