using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;
using Xunit;

namespace VendorMdm.Api.Tests.Services
{
    public class UserServiceTests
    {
        private readonly UserService _service;
        private readonly SqlDbContext _context;
        private readonly Mock<ICosmosRepository> _mockCosmos;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<SqlDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new SqlDbContext(options);
            _mockCosmos = new Mock<ICosmosRepository>();

            var logger = new Mock<ILogger<UserService>>();
            
            _service = new UserService(_context, _mockCosmos.Object, logger.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldAssignRole_WhenProvided()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Roles = new List<string> { "Admin" }
            };

            // Act
            var result = await _service.CreateUserAsync(user);

            // Assert
            Assert.Contains("Admin", result.Roles);
            Assert.Equal("Local", result.AuthProvider);
            Assert.NotNull(_context.Users.FirstOrDefault(u => u.Email == "test@example.com"));
        }

        [Fact]
        public async Task CreateUser_ShouldDefaultToViewer_WhenRoleMissing()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser2",
                Email = "test2@example.com",
                Roles = new List<string>() // Empty
            };

            // Act
            var result = await _service.CreateUserAsync(user);

            // Assert
            Assert.Contains("Viewer", result.Roles);
        }

        [Fact]
        public async Task UpdateUserRoles_ShouldUpdateRoles()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser3",
                Email = "test3@example.com",
                Roles = new List<string> { "Viewer" }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var updatedUser = await _service.UpdateUserRolesAsync(user.Id, new List<string> { "Approver" });

            // Assert
            Assert.Contains("Approver", updatedUser!.Roles);
            var dbUser = await _context.Users.FindAsync(user.Id);
            Assert.Contains("Approver", dbUser!.Roles);
        }
    }
}
