using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Articles;
using MagMini.Application.DTOs.Customers;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Entities;
using MagMini.Domain.Enums;

namespace MagMini.UI.ViewModels;

public partial class OrderEditViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IArticleService _articleService;

    [ObservableProperty]
    private SaveOrderDto _order = new();

    [ObservableProperty]
    private ObservableCollection<OrderItemDto> _items = new();

    [ObservableProperty]
    private OrderItemDto? _selectedItem;

    [ObservableProperty]
    private List<CustomerDto> _customers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private List<ArticleDto> _articles = new();

    [ObservableProperty]
    private ArticleDto? _selectedArticleToAdd;

    [ObservableProperty]
    private decimal _quantityToAdd = 1;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _windowTitle = "Nowe Zamówienie";

    public decimal TotalNet => Items.Sum(i => i.TotalNet);
    public decimal TotalGross => Items.Sum(i => i.TotalGross);
    public decimal TotalVat => TotalGross - TotalNet;

    public event Action? SavedSuccessfully;

    public OrderEditViewModel(IOrderService orderService, ICustomerService customerService, IArticleService articleService)
    {
        _orderService = orderService;
        _customerService = customerService;
        _articleService = articleService;
    }

    public async Task InitializeAsync(int orderId = 0)
    {
        ErrorMessage = null;

        // Załaduj klientów i towary do list wyboru
        var custResult = await _customerService.GetPagedAsync(new CustomerFilterDto { PageSize = 1000 });
        Customers = custResult.Items.ToList();

        var artResult = await _articleService.GetPagedAsync(new ArticleFilterDto { PageSize = 1000 });
        Articles = artResult.Items.ToList();
        SelectedArticleToAdd = Articles.FirstOrDefault();

        if (orderId > 0)
        {
            WindowTitle = "Edycja Zamówienia";
            var existing = await _orderService.GetForEditAsync(orderId);
            if (existing != null)
            {
                Order = existing;
                Items = new ObservableCollection<OrderItemDto>(existing.Items);
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == Order.CustomerId);
            }
        }
        else
        {
            WindowTitle = "Nowe Zamówienie";
            var nextNum = await _orderService.GenerateNextOrderNumberAsync();
            Order = new SaveOrderDto
            {
                OrderNumber = nextNum,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Draft
            };
            Items = new ObservableCollection<OrderItemDto>();
            SelectedCustomer = Customers.FirstOrDefault();
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedArticleToAdd == null || QuantityToAdd <= 0) return;

        var existing = Items.FirstOrDefault(i => i.ArticleId == SelectedArticleToAdd.Id);
        if (existing != null)
        {
            existing.Quantity += QuantityToAdd;
        }
        else
        {
            Items.Add(new OrderItemDto
            {
                ArticleId = SelectedArticleToAdd.Id,
                ArticleCode = SelectedArticleToAdd.Code,
                ArticleName = SelectedArticleToAdd.Name,
                Quantity = QuantityToAdd,
                UnitPriceNet = SelectedArticleToAdd.DefaultSalePriceNet,
                VatRate = SelectedArticleToAdd.VatRate
            });
        }

        QuantityToAdd = 1;
        RecalculateTotals();
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem == null) return;
        Items.Remove(SelectedItem);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        OnPropertyChanged(nameof(TotalNet));
        OnPropertyChanged(nameof(TotalGross));
        OnPropertyChanged(nameof(TotalVat));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        if (SelectedCustomer == null)
        {
            ErrorMessage = "Wybierz kontrahenta!";
            return;
        }

        if (!Items.Any())
        {
            ErrorMessage = "Dodaj co najmniej jeden artykuł do zamówienia!";
            return;
        }

        Order.CustomerId = SelectedCustomer.Id;
        Order.Items = Items.ToList();

        var result = await _orderService.SaveAsync(Order);
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