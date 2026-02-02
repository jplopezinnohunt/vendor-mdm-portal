using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VendorMdm.Core.Framework.Primitives;

namespace VendorMdm.Core.Framework.Ontology
{
    /// <summary>
    /// Represents a Canonical Entity in the Ontology Layer.
    /// All business logic must reside within implementations of this interface.
    /// </summary>
    public interface IOntologyConcept
    {
        /// <summary>
        /// Unique Identifier for the Concept Instance.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The Context (App/Service) where this data originated.
        /// </summary>
        string OriginContext { get; }

        /// <summary>
        /// Validates the current state of the concept against business rules.
        /// </summary>
        Result ValidateState();

        /// <summary>
        /// Emits functional logs for Observability.
        /// </summary>
        IDictionary<string, object> GetFunctionalLogs();

        /// <summary>
        /// Returns Domain Events that occurred during the concept's lifecycle.
        /// </summary>
        IEnumerable<object> GetDomainEvents();
    }
}
