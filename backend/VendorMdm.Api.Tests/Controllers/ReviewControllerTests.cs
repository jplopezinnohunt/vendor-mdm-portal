using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VendorMdm.Api.Controllers;
using VendorMdm.Api.Data;
using VendorMdm.Api.Models;
using VendorMdm.Shared.Models;
using Xunit;

namespace VendorMdm.Api.Tests.Controllers
{
    public class ReviewControllerTests
    {
        private readonly SqlDbContext _context;
        private readonly Mock<ILogger<ReviewController>> _loggerMock;
        private readonly ReviewController _controller;

        public ReviewControllerTests()
        {
            var options = new DbContextOptionsBuilder<SqlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SqlDbContext(options);
            _loggerMock = new Mock<ILogger<ReviewController>>();
            _controller = new ReviewController(_context, _loggerMock.Object);
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
            var result = await _controller.ApproveApplication(appId, request);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            
            var updatedApp = await _context.VendorApplications.FindAsync(appId);
            Assert.NotNull(updatedApp);
            Assert.Equal("Approved", updatedApp.Status);
            
            // Check core field update
            Assert.Equal("New Name", updatedApp.CompanyName);
            
            // Check attributes merge
            var updatedAttributes = JsonSerializer.Deserialize<Dictionary<string, object>>(updatedApp.Attributes);
            Assert.NotNull(updatedAttributes);
            
            Assert.True(updatedAttributes.ContainsKey("taxCode1"));
            Assert.Equal("10", updatedAttributes["taxCode1"].ToString());
            
            Assert.True(updatedAttributes.ContainsKey("accountGroup"));
            Assert.Equal("INDV", updatedAttributes["accountGroup"].ToString());
            
            Assert.True(updatedAttributes.ContainsKey("existingKey"));
            Assert.Equal("updatedValue", updatedAttributes["existingKey"].ToString());
        }
    }
}
