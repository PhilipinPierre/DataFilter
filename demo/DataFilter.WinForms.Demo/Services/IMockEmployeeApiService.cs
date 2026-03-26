using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.WinForms.Demo.Services;

public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>
{
    void Regenerate(int count);
}
