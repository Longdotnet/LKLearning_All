using CommunityToolkit.Maui.Views;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Views.UserProfilePage
{
    public partial class UserProfilePage : ContentPage
    {
        double lastScrollY = 0;
        bool headerHidden = false;

        public UserProfilePage()
        {
            InitializeComponent();
            var vm = BindingContext as UserProfileViewModel;
            if (vm != null)
            {
                vm.ShowAvatarPopup += OnShowAvatarPopup;
            }
        }

        private void OnShowAvatarPopup(System.Collections.ObjectModel.ObservableCollection<Models.Avatar> avatars)
        {
            var vm = BindingContext as UserProfileViewModel;
            var popup = new AvatarSelectionPopup(avatars, vm.SelectedAvatarId, selectedAvatar =>
            {
                vm.AvatarUrl = selectedAvatar.UrlPath;
                vm.SelectedAvatarId = selectedAvatar.AvatarId;
            });
            this.ShowPopup(popup);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void MainScroll_Scrolled(object sender, ScrolledEventArgs e)
        {
            double delta = e.ScrollY - lastScrollY;
            lastScrollY = e.ScrollY;

            if (delta > 0 && !headerHidden && e.ScrollY > 50)
            {
                headerHidden = true;
                await HeaderContainer.TranslateTo(0, -HeaderContainer.Height, 250, Easing.CubicOut);
            }
            else if (delta < 0 && headerHidden)
            {
                headerHidden = false;
                await HeaderContainer.TranslateTo(0, 0, 250, Easing.CubicOut);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MainScroll.Scrolled += MainScroll_Scrolled;
            if (BindingContext is UserProfileViewModel viewModel)
            {
                await viewModel.FetchUserDataAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MainScroll.Scrolled -= MainScroll_Scrolled;
        }
    }
}