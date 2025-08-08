using CommunityToolkit.Maui.Views;
using mauiluanvantotnghiep.ViewModels.Popup;

namespace mauiluanvantotnghiep.ViewsPopup
{
    public partial class AlertLessonExam : Popup
    {
        public AlertLessonExam(int courseId)
        {
            InitializeComponent();
            BindingContext = new AlertLessonExamViewModel(courseId, ClosePopup);
        }

        private void ClosePopup()
        {
            Close();
        }
    }
}