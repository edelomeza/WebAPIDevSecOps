using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using WebAPIDevSecOps.Controllers;
using WebAPIDevSecOps.Services;

namespace UnitTest.Login
{
    public class LogoutTestUnit
    {
        private static LogoutController CreateController(string? token)
        {
            var controller = new LogoutController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            if (!string.IsNullOrEmpty(token))
            {
                controller.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            return controller;
        }

        [Fact]
        public void Logout_ReturnsOk_WhenTokenProvided()
        {
            var controller = CreateController("test-token");
            TokenBlacklist.CleanupExpired();

            var result = controller.Logout();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Logout_ReturnsUnauthorized_WhenNoToken()
        {
            var controller = CreateController(null);

            var result = controller.Logout();

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public void Logout_AddsTokenToBlacklist()
        {
            var token = "token-to-blacklist";
            var controller = CreateController(token);
            TokenBlacklist.CleanupExpired();

            controller.Logout();

            Assert.True(TokenBlacklist.IsBlacklisted(token));
        }
    }
}
