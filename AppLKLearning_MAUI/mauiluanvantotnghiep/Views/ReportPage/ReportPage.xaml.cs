using mauiluanvantotnghiep.ViewModels.ReviewViewModel;

namespace mauiluanvantotnghiep.Views.ReportPage
{


    [QueryProperty(nameof(LessonId), "lessonId")]
    [QueryProperty(nameof(CourseId), "courseId")]
    public partial class ReportPage : ContentPage
    {


        int courseId;
        int lessonId;

        public int CourseId
        {
            get => courseId;
            set
            {
                courseId = value;
                OnPropertyChanged();

                if (BindingContext is ReportViewModel vm)
                    vm.CourseId = courseId;
            }
        }

        public int LessonId
        {
            get => lessonId;
            set
            {
                lessonId = value;
                OnPropertyChanged();

                if (BindingContext is ReportViewModel vm)
                    vm.LessonId = lessonId;
            }
        }
        public ReportPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ReportViewModel vm)
            {
                await vm.LoadReportsAsync();
            }
        }
    }
}