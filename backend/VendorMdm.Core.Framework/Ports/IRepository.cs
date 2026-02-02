using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VendorMdm.Core.Framework.Ports
{
    /// <summary>
    /// Repository abstraction for Hexagonal Architecture.
    /// Separates domain logic from data access.
    /// </summary>
    public interface IRepository<TConcept> where TConcept : Ontology.IOntologyConcept
    {
        Task<TConcept?> GetByIdAsync(Guid id);
        Task<IEnumerable<TConcept>> GetAllAsync();
        Task<IEnumerable<TConcept>> FindAsync(Func<TConcept, bool> predicate);
        Task SaveAsync(TConcept concept);
        Task DeleteAsync(Guid id);
    }
}
