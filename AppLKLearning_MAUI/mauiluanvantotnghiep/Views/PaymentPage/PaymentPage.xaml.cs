using Microsoft.Maui.Controls;
using mauiluanvantotnghiep.ViewModels;
using System;

namespace mauiluanvantotnghiep.Views.PaymentPage
{
    [QueryProperty(nameof(PaymentUrl), "paymentUrl")]
    [QueryProperty(nameof(OrderId), "orderId")]
    [QueryProperty(nameof(PaymentMethod), "paymentMethod")]
    public partial class PaymentPage : ContentPage
    {
        private readonly PaymentPageViewModel _viewModel;

        private string _paymentUrl;
        public string PaymentUrl
        {
            get => _paymentUrl;
            set
            {
                _paymentUrl = Uri.UnescapeDataString(value);
                if (_viewModel != null)
                {
                    _viewModel.PaymentUrl = _paymentUrl;
                }
            }
        }

        private string _orderId;
        public string OrderId
        {
            get => _orderId;
            set
            {
                _orderId = !string.IsNullOrEmpty(value) ? Uri.UnescapeDataString(value) : value;
                if (_viewModel != null)
                {
                    _viewModel.OrderId = _orderId;
                }
            }
        }

        private string _paymentMethod;
        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = !string.IsNullOrEmpty(value) ? Uri.UnescapeDataString(value) : value;
                if (_viewModel != null)
                {
                    _viewModel.PaymentMethod = _paymentMethod;
                }
            }
        }

        public PaymentPage()
        {
            InitializeComponent();
            _viewModel = new PaymentPageViewModel();
            BindingContext = _viewModel;

            // Đăng ký sự kiện Navigating
            PaymentWebView.Navigating += (s, e) => _viewModel.HandleWebNavigatingCommand.Execute(e);
        }
    }
}