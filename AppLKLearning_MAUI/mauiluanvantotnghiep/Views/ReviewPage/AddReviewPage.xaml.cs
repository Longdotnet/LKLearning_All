using mauiluanvantotnghiep.ViewModels;
using mauiluanvantotnghiep.Models;

namespace mauiluanvantotnghiep.Views.ReviewPage
{
    [QueryProperty(nameof(CourseId), "courseId")]
    [QueryProperty(nameof(ExistingReview), "existingReview")]
    public partial class AddReviewPage : ContentPage
    {
        int _courseId;
        public int CourseId
        {
            get => _courseId;
            set => _courseId = value;
        }

        Review _existingReview;
        public Review ExistingReview
        {
            get => _existingReview;
            set => _existingReview = value;
        }

        public AddReviewPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is AddReviewPageViewModel vm && _courseId > 0)
            {
                await vm.InitializeAsync(_courseId, _existingReview);
            }
        }
    }
}