using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VendorMdm.Api.Data;

namespace VendorMdm.Api.Tests.Helpers;

public class TestBase
{
    protected SqlDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<SqlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        return new SqlDbContext(options);
    }
    
    protected Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }
}
