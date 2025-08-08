using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Views.LessonPage
{
    // Sử dụng QueryProperty truyền CourseId từ URL query "courseId"
    [QueryProperty("CourseId", "courseId")]

    public partial class LessonPage : ContentPage
    {
        private int courseId;
        public int CourseId
        {
            get => courseId;
            set
            {
                courseId = value;
                // Khi courseId được gán, gọi load dữ liệu thông qua ViewModel
                if (BindingContext is LessonPageViewModel vm)
                {
                    _ = vm.LoadLessonsByCourseAsync(courseId);
                }
            }
        }

        public LessonPage()
        {
            InitializeComponent();
            // Gán BindingContext (không cần truyền CourseId tại đây vì shell sẽ truyền qua query)
            BindingContext = new LessonPageViewModel();
        }


    }
}