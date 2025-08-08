using System.Diagnostics;
using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;
namespace mauiluanvantotnghiep.Views.PaymentPage;

[QueryProperty(nameof(PackageId), "packageId")]
[QueryProperty(nameof(Price), "price")]
public partial class PaymentgatewayPage : ContentPage
{
    public string PackageId { get; set; } // Đổi sang string để khớp với Dictionary<string, object>
    public string Price { get; set; }    // Đổi sang string để khớp với Dictionary<string, object>
    public PaymentgatewayPage()
	{
		InitializeComponent();
        BindingContext = new PaymentGatewayPageViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine($"OnAppearing: PackageId={PackageId}, Price={Price}");

        if (BindingContext is PaymentGatewayPageViewModel viewModel)
        {
            // Kiểm tra và chuyển đổi PackageId
            if (int.TryParse(PackageId, out int packageId))
            {
                viewModel.PackageId = packageId;
            }
            else
            {
                viewModel.PackageId = 0; // Giá trị mặc định
                System.Diagnostics.Debug.WriteLine("PackageId không hợp lệ hoặc null");
            }

            // Kiểm tra và chuyển đổi Price
            if (decimal.TryParse(Price, out decimal price))
            {
                viewModel.Price = price;
            }
            else
            {
                viewModel.Price = 0; // Giá trị mặc định
                System.Diagnostics.Debug.WriteLine("Price không hợp lệ hoặc null");
            }
        }
    }
}