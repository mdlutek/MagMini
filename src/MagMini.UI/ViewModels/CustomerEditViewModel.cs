using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.Common.Validators;
using MagMini.Application.DTOs.Customers;

namespace MagMini.UI.ViewModels;

public partial class CustomerEditViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly ICompanyLookupService _companyLookupService;

    [ObservableProperty]
    private SaveCustomerDto _customer = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successInfo;

    [ObservableProperty]
    private bool _isFetchingGus;

    [ObservableProperty]
    private string _windowTitle = "Nowy Kontrahent";

    public event Action? SavedSuccessfully;

    public CustomerEditViewModel(ICustomerService customerService, ICompanyLookupService companyLookupService)
    {
        _customerService = customerService;
        _companyLookupService = companyLookupService;
    }

    public async Task InitializeAsync(int customerId = 0)
    {
        ErrorMessage = null;
        SuccessInfo = null;
        if (customerId > 0)
        {
            WindowTitle = "Edycja Kontrahenta";
            var existing = await _customerService.GetForEditAsync(customerId);
            if (existing != null) Customer = existing;
        }
        else
        {
            WindowTitle = "Nowy Kontrahent";
            Customer = new SaveCustomerDto { IsCompany = true };
        }
    }

    [RelayCommand]
    private async Task FetchFromGusAsync()
    {
        ErrorMessage = null;
        SuccessInfo = null;

        if (string.IsNullOrWhiteSpace(Customer.Nip))
        {
            ErrorMessage = "Wpisz NIP przed pobraniem danych!";
            return;
        }

        if (!NipValidator.IsValid(Customer.Nip))
        {
            ErrorMessage = "Nieprawidłowy format NIP (błędna suma kontrolna)!";
            return;
        }

        IsFetchingGus = true;
        try
        {
            var result = await _companyLookupService.LookupByNipAsync(Customer.Nip);
            if (result.IsSuccess)
            {
                // Zawsze generujemy symbol pasujący do nowo pobranej firmy
                var cleanName = new string(result.Name.Where(char.IsLetterOrDigit).ToArray());
                var generatedCode = cleanName.Length > 10 ? cleanName.Substring(0, 10).ToUpper() : cleanName.ToUpper();

                // Tworzymy nowy obiekt - dzięki temu WPF natychmiast odświeża wszystkie kontrolki w UI
                Customer = new SaveCustomerDto
                {
                    Id = Customer.Id,
                    IsCompany = true,
                    Nip = result.Nip,
                    Code = generatedCode,
                    Name = result.Name,
                    Street = result.Street,
                    PostalCode = result.PostalCode,
                    City = result.City,
                    Phone = Customer.Phone, // zachowujemy ewentualnie wpisany wcześniej telefon/email
                    Email = Customer.Email
                };

                SuccessInfo = $"Pobrano dane z rejestru MF! (Status VAT: {result.StatusVat ?? "Brak danych"})";
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsFetchingGus = false;
        }
    }
    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        SuccessInfo = null;

        if (string.IsNullOrWhiteSpace(Customer.Code) || string.IsNullOrWhiteSpace(Customer.Name))
        {
            ErrorMessage = "Symbol i Nazwa są polami wymaganymi!";
            return;
        }

        var result = await _customerService.SaveAsync(Customer);
        if (result.Success)
        {
            SavedSuccessfully?.Invoke();
        }
        else
        {
            ErrorMessage = result.Error;
        }
    }
}