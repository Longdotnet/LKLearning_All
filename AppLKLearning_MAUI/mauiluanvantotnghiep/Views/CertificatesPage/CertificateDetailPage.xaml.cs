using mauiluanvantotnghiep.ViewModels.CertificatesPageViewModel;

namespace mauiluanvantotnghiep.Views.CertificatesPage;

[QueryProperty(nameof(CertificateId), "certificateId")]
public partial class CertificateDetailPage : ContentPage
{
    public string CertificateId { get; set; }

    public CertificateDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is CertificateDetailViewModel viewModel && !string.IsNullOrEmpty(CertificateId))
        {
            if (int.TryParse(CertificateId, out int certId))
            {
                await viewModel.LoadCertificateDetailAsync(certId);
            }
        }
    }
}