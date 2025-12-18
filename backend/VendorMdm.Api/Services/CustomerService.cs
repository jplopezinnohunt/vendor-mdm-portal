using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        SqlDbContext context,
        CosmosRepository cosmosRepository,
        ILogger<CustomerService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        // 1. SQL Persistence
        customer.Id = Guid.NewGuid();
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.EntityVersion = 1;
        customer.SchemaVersion = "v1.0.0";
        if (string.IsNullOrEmpty(customer.Status)) customer.Status = "Active";
        if (string.IsNullOrEmpty(customer.SourceSystem)) customer.SourceSystem = SourceSystems.GetDefaultSource(typeof(Customer));

        customer.ValidateCanonicalFields();

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(customer.Id.ToString(), customer);

        // 3. Event
        var domainEvent = new DomainEvent
        {
            EventType = "CustomerCreated",
            EntityId = customer.Id.ToString(),
            Data = customer,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = customer.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        customer.IncrementVersion();

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();

        await _cosmosRepository.SaveArtifactAsync(customer.Id.ToString(), customer);

        var domainEvent = new DomainEvent
        {
            EventType = "CustomerUpdated",
            EntityId = customer.Id.ToString(),
            Data = customer,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = customer.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return customer;
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.ToListAsync();
    }
}
