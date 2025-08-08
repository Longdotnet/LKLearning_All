using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels;
using System.Collections.Generic;

namespace mauiluanvantotnghiep.Views.ReviewPage
{
    [QueryProperty(nameof(CourseId), "courseId")]
    public partial class ReviewDetailPage : ContentPage
    {
        int _courseId;
        public int CourseId
        {
            get => _courseId;
            set => _courseId = value;
        }

        public ReviewDetailPage()
        {
            InitializeComponent();
            BindingContext = new ReviewDetailViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is ReviewDetailViewModel vm && _courseId > 0)
            {
                await vm.LoadAllReviewsAsync(_courseId); // Làm mới danh sách nhận xét
                if (vm.SelectedFilter == ReviewFilter.Mine)
                {
                    await vm.LoadReviewStatusAsync(); // Làm mới trạng thái và nút
                }
            }
        }
    }
}