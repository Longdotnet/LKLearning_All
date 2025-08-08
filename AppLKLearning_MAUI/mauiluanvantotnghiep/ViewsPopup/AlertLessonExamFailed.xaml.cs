using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.ViewsPopup
{

    public partial class AlertLessonExamFailed : Popup
    {
        public AlertLessonExamFailed()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}