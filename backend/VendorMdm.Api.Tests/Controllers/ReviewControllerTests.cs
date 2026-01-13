using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VendorMdm.Api.Controllers;
using VendorMdm.Api.Data;
using VendorMdm.Api.Models;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;
using Xunit;

namespace VendorMdm.Api.Tests.Controllers
{
    public class ReviewControllerTests
    {
        private readonly SqlDbContext _context;
        private readonly Mock<ILogger<ReviewController>> _loggerMock;
        private readonly Mock<IVendorApplicationService> _vendorServiceMock;
        private readonly ReviewController _controller;

        public ReviewControllerTests()
        {
            var options = new DbContextOptionsBuilder<SqlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SqlDbContext(options);
            _loggerMock = new Mock<ILogger<ReviewController>>();
            _vendorServiceMock = new Mock<IVendorApplicationService>();
            _controller = new ReviewController(_context, _vendorServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ApproveApplication_WithEnrichedAttributes_UpdatesAndMergesData()
        {
            // Arrange
            var appId = Guid.NewGuid();
            var attributes = new Dictionary<string, object>
            {
                { "existingKey", "existingValue" }
            };
            
            var app = new VendorApplication
            {
                Id = appId,
                CompanyName = "Original Name",
                TaxId = "TAX-123",
                ContactName = "John Doe",
                ContactEmail = "john@example.com",
                Status = "PendingReview",
                Attributes = JsonSerializer.Serialize(attributes)
            };

            _context.VendorApplications.Add(app);
            await _context.SaveChangesAsync();

            var request = new ApprovalRequest
            {
                Comments = "Approved",
                EnrichedAttributes = new Dictionary<string, object>
                {
                    { "companyName", "New Name" }, // Should update core column
                    { "taxCode1", "10" },          // New enriched attribute
                    { "accountGroup", "INDV" },    // New enriched attribute
                    { "existingKey", "updatedValue" } // Update existing attribute
                }
            };

            // Act
            _vendorServiceMock.Setup(x => x.ApproveApplicationAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Set ControllerContext to avoid NullReference on User.Identity
            _controller.ControllerContext = new ControllerContext 
            { 
                HttpContext = new DefaultHttpContext() 
            };

            // Act
            var result = await _controller.ApproveApplication(appId, request);

            // Assert
            // Controller returns Ok(...) which is OkObjectResult. 
            // OkObjectResult inherits from ObjectResult.
            // If it returns 500, it returns ObjectResult. 
            // We check for 200 first.
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            
            _vendorServiceMock.Verify(x => x.ApproveApplicationAsync(
                appId, 
                It.Is<Dictionary<string, object>>(d => d["companyName"].ToString() == "New Name" && d.ContainsKey("taxCode1")),
                false, 
                "System" // User.Identity is null in test so defaults to System
            ), Times.Once);
        }
    }
}
