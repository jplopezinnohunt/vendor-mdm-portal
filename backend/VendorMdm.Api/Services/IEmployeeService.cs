using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface IEmployeeService
{
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task<Employee> UpdateEmployeeAsync(Employee employee);
    Task<Employee?> GetEmployeeByIdAsync(Guid id);
    Task<List<Employee>> GetAllEmployeesAsync();
}
