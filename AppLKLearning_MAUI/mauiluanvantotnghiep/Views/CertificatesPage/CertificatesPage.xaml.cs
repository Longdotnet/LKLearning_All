using mauiluanvantotnghiep.ViewModels;
using mauiluanvantotnghiep.ViewModels.CertificatesPageViewModel;

namespace mauiluanvantotnghiep.Views.CertificatesPage;

public partial class CertificatesPage : ContentPage
{
    public CertificatesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CertificatesPageViewModel viewModel)
        {
            await viewModel.LoadCertificatesAsync();
        }
    }
}