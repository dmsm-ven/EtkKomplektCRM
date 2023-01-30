
using EtkBlazorAppi.Core.Data;
using EtkBlazorAppi.Core.Data.CompanyInfo;

namespace EtkBlazorApp.Core.Interfaces;

public interface ICompanyInfoChecker
{
    Task<CompanyInformation?> GetInfoByInn(string inn);
}