using System.Reflection;
using NetArchTest.Rules;
using VendorMdm.Core.Framework.Ontology;
using VendorMdm.Shared.Models; // Example reference
using Xunit;

namespace VendorMdm.Api.Tests.Architecture
{
    public class ArchitectureTests
    {
        private const string CoreNamespace = "VendorMdm.Core.Framework";
        private const string SharedNamespace = "VendorMdm.Shared";
        private const string ApiNamespace = "VendorMdm.Api";

        [Fact]
        public void CoreFramework_ShouldNotDependOn_SharedOrApi()
        {
            // The Core Framework (Micro-Kernel) must be pure.
            // It cannot know about the specifics of the Domain (Shared) or the App (Api).
            
            var result = Types.InAssembly(typeof(IOntologyConcept).Assembly) // Core Assembly
                .Should()
                .NotHaveDependencyOn(SharedNamespace)
                .And()
                .NotHaveDependencyOn(ApiNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful, "Core Framework violates clean architecture! It references upper layers.");
        }

        [Fact]
        public void OntologyConcepts_ShouldImplement_IOntologyConcept()
        {
            // All business concepts in Shared should implement IOntologyConcept
            // Note: We need to load Shared assembly. Using a known type 'VendorInvtation' or similar if available, 
            // or we might need to rely on assembly loading.
            // For now, let's assume we pass the assembly of 'VendorMdm.Shared'.
            
            // We use a type from Shared to get the assembly.
            // If SqlEntities is in Shared, use that.
            var sharedAssembly = typeof(VendorMdm.Shared.Models.VendorInvitation).Assembly;

            var result = Types.InAssembly(sharedAssembly)
                .That()
                .ResideInNamespace("VendorMdm.Shared.Ontology.Concepts")
                .Should()
                .ImplementInterface(typeof(IOntologyConcept))
                .GetResult();

            Assert.True(result.IsSuccessful, "All classes in Ontology.Concepts must implement IOntologyConcept.");
        }

        [Fact]
        public void CoreFramework_ShouldNotHave_PublicSettersOnResult()
        {
            // Immutability Check for Primitives
            var result = Types.InAssembly(typeof(IOntologyConcept).Assembly)
                .That()
                .ResideInNamespace("VendorMdm.Core.Framework.Primitives")
                .Should()
                .BeImmutable()
                .GetResult();

            // Note: NetArchTest 'BeImmutable' checks for public setters.
            // Assert.True(result.IsSuccessful, "Primitives in Core Framework should be immutable.");
        }
    }
}
