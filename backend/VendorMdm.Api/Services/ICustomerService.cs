using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface ICustomerService
{
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<Customer?> GetCustomerByIdAsync(Guid id);
    Task<List<Customer>> GetAllCustomersAsync();
}
