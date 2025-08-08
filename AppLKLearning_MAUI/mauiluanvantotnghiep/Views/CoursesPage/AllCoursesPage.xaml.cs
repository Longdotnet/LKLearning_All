using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace mauiluanvantotnghiep.Views.CoursesPage
{
    [QueryProperty(nameof(PackageId), "packageId")]
    public partial class AllCoursesPage : ContentPage
    {
        private readonly AllCoursesViewModel vm;
        private int packageId;

        public AllCoursesPage()
        {
            InitializeComponent();

            // 1) Khởi tạo ViewModel và gán BindingContext
            vm = new AllCoursesViewModel();
            BindingContext = vm;

            Debug.WriteLine("[DEBUG] AllCoursesPage ctor, packageId field = " + packageId);
        }

        public int PackageId
        {
            get => packageId;
            set
            {
                packageId = value;
                Debug.WriteLine("[DEBUG] AllCoursesPage got packageId = " + packageId);

                // 2) Gọi load dữ liệu ngay khi param được set
                _ = vm.LoadAsync(packageId);
            }
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // REMOVE: Không dispose khi page disappear
            // vm?.Dispose();
        }

        // Chỉ dispose khi page thực sự bị destroy
        ~AllCoursesPage()
        {
            vm?.Dispose();
        }
    }
}