using System;
using VendorMdm.Shared.Ontology.Concepts;
using Xunit;

namespace VendorMdm.Api.Tests.Ontology
{
    public class ConceptTests
    {
        [Fact]
        public void VendorConcept_ShouldDetermineAccountGroup_Correctly()
        {
            // Arrange
            var vendor = new VendorConcept("Acme Corp", "Organization", "Test");

            // Act
            var group = vendor.AccountGroup;
            var validation = vendor.ValidateState();

            // Assert
            Assert.Equal("LIFNR", group); // Rule: Organization -> LIFNR
            Assert.True(validation.IsSuccess);
        }

        [Fact]
        public void EventConcept_ShouldFail_IfDatesInvalid()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(10);
            var end = DateTime.UtcNow.AddDays(5); // End before Start!
            var evt = new EventConcept("Bad Event", start, end, "Test");

            // Act
            var result = evt.ValidateState();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Start Date must be before End Date.", result.Error);
        }

        [Fact]
        public void ChangeRequest_ShouldRequireVendorId()
        {
            // Arrange
            var cr = new ChangeRequestConcept(Guid.Empty, "BankUpdate", "Test");

            // Act
            var result = cr.ValidateState();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Vendor ID is required", result.Error);
        }
    }
}
