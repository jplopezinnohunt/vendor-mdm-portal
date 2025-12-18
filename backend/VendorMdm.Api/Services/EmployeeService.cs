using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        SqlDbContext context,
        CosmosRepository cosmosRepository,
        ILogger<EmployeeService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        if (employee == null) throw new ArgumentNullException(nameof(employee));

        // 1. SQL Persistence
        employee.Id = Guid.NewGuid();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.EntityVersion = 1;
        employee.SchemaVersion = "v1.0.0";
        if (string.IsNullOrEmpty(employee.Status)) employee.Status = "Active";
        if (string.IsNullOrEmpty(employee.SourceSystem)) employee.SourceSystem = SourceSystems.GetDefaultSource(typeof(Employee));
        
        employee.ValidateCanonicalFields();

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(employee.Id.ToString(), employee);

        // 3. Event
        var domainEvent = new DomainEvent
        {
            EventType = "EmployeeCreated",
            EntityId = employee.Id.ToString(),
            Data = employee,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = employee.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        employee.IncrementVersion();

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        await _cosmosRepository.SaveArtifactAsync(employee.Id.ToString(), employee);

        var domainEvent = new DomainEvent
        {
            EventType = "EmployeeUpdated",
            EntityId = employee.Id.ToString(),
            Data = employee,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = employee.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return employee;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        return await _context.Employees.FindAsync(id);
    }
    
    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees.ToListAsync();
    }
}
