using Microsoft.Maui.Controls;
using mauiluanvantotnghiep.ViewModels;
using mauiluanvantotnghiep.Models;

namespace mauiluanvantotnghiep.Views.TransactionPage
{
    public partial class TransactionsPage : ContentPage
    {
        public TransactionsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var vm = BindingContext as TransactionsViewModel;
            vm?.StartBannerTimer();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var vm = BindingContext as TransactionsViewModel;
            vm?.StopBannerTimer();
        }

    
    }
}