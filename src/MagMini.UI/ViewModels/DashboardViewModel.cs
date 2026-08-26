using CommunityToolkit.Mvvm.ComponentModel;
using MagMini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagMini.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private int _articlesCount;

    [ObservableProperty]
    private int _customersCount;

    [ObservableProperty]
    private int _ordersCount;

    [ObservableProperty]
    private decimal _totalSalesGross;

    public DashboardViewModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task LoadStatisticsAsync()
    {
        ArticlesCount = await _context.Articles.CountAsync();
        CustomersCount = await _context.Customers.CountAsync();
        OrdersCount = await _context.Orders.CountAsync();

        var orders = await _context.Orders.Include(o => o.Items).ToListAsync();
        TotalSalesGross = orders.Sum(o => o.TotalGross);
    }
}