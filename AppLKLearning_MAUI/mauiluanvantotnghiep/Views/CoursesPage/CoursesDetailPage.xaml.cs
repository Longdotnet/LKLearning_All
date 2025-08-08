using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels;

using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Views.CoursesPage
{
    [QueryProperty(nameof(CourseId), "courseId")]
    public partial class CoursesDetailPage : ContentPage
    {
        private int courseId;
        public int CourseId
        {
            get => courseId;
            set
            {
                courseId = value;
                if (BindingContext is CourseDetailViewModel vm)
                {
                    _ = vm.LoadCourseDetailAsync(courseId);
                    _ = vm.LoadReviewsAsync();
                }
            }
        }

        public CoursesDetailPage()
        {
            InitializeComponent();
            BindingContext = new CourseDetailViewModel();
        }
    }


}
