
using EtkBlazorAppi.Core.Data;

namespace EtkBlazorApp.Core.Interfaces;

public interface ICompanyInfoChecker
{
    Task<CompanyInformation?> GetInfoByInn(string inn);
}