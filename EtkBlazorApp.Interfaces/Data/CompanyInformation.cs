namespace EtkBlazorAppi.Core.Data;

public class CompanyInformation
{
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RegistrationDate { get; set; } = string.Empty;
    public string Capital { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public CompanyManagmentInformation Managment { get; set; } = new();
    public CompanyGeneralCodes Codes { get; set; } = new();
    public CompanyFinanceInformation FinanceInfo { get; set; } = new();
}

public class CompanyManagmentInformation
{
    public string Name { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public string Disqualified { get; set; } = string.Empty;
}

public class CompanyGeneralCodes
{
    public string Kpp { get; set; } = string.Empty;
    public string Inn { get; set; } = string.Empty;
    public string Ogrn { get; set; } = string.Empty;
    public string Okpo { get; set; } = string.Empty;
    public string Okato { get; set; } = string.Empty;
    public string Oktmo { get; set; } = string.Empty;
    public string Okogu { get; set; } = string.Empty;
    public string Okfs { get; set; } = string.Empty;
    public string Okved { get; set; } = string.Empty;
}

public class CompanyFinanceInformation
{
    public string Income { get; set; } = string.Empty;
    public string Expense { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}