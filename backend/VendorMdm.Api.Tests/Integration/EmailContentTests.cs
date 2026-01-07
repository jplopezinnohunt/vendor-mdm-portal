using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;
using Xunit;
using MimeKit;
using System.IO;
using VendorMdm.Api.Tests.Helpers;

namespace VendorMdm.Api.Tests.Integration;

public class EmailContentTests
{
    [Fact]
    public void Verify_Mfa_Email_Content_Structure()
    {
        // 1. Setup Service (Partial Mocking to test private method logic via public interface if possible, 
        // or just by inspecting what would be sent to SMTP)
        
        // Since BuildEmailHtml is private, we can't test it directly without reflection or making it internal.
        // However, we can use the "TrySendViaSmtpAsync" logic by mocking the SMTP client OR 
        // simply by verifying the "Console Output" fallback logic which prints the details.
        
        // Actually, a better way is to instantiate the EmailService and call SendMfaCodeEmailAsync 
        // with a mock SMTP configuration that fails, thus falling back to LogEmail, 
        // OR better yet, extract the template building to a testable helper.
        
        // BUT, for this verify task, I want to ensure the HTML IS VALID.
        // I will use `Reflection` to invoke the private `BuildEmailHtml` method to verify the content.
        
        var service = new EmailService(
            new Mock<IHttpClientFactory>().Object,
            MockHelpers.CreateMockConfiguration(),
            new Mock<ILogger<EmailService>>().Object
        );
        
        var method = typeof(EmailService).GetMethod("BuildEmailHtml", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        var data = new InvitationEmailData 
        { 
            VendorName = "Test Vendor",
            Email = "test@vendor.com",
            Token = "test-token",
            ExpiresAt = System.DateTime.UtcNow.AddMinutes(15)
        };
        
        // Invoke
        var result = (string)method.Invoke(service, new object[] { 
            data, 
            "http://localhost/link", 
            "Expiration String", 
            "UNESCO", 
            true, // isMfa
            "123456" // code
        });
        
        // Assert
        result.Should().Contain("Identity Verification");
        result.Should().Contain("123456");
        result.Should().Contain("Test Vendor");
        result.Should().Contain("If you did not request this code");
    }
}
