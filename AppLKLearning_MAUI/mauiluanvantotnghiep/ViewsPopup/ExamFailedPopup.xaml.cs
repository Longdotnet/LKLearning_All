using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.ViewsPopup
{
    public partial class ExamFailedPopup : Popup
    {
        public decimal TotalScore { get; }
        public decimal PassingScore { get; }
        public int CourseId { get; }

        public ExamFailedPopup(decimal totalScore, decimal passingScore, int courseId)
        {
            InitializeComponent();
            
            TotalScore = totalScore;
            PassingScore = passingScore;
            CourseId = courseId;
            
            InitializeContent();
            SetupEventHandlers();
        }

        private void InitializeContent()
        {
            ScoreLabel.Text = $"Điểm của bạn: {TotalScore:0.#} điểm";
            PassingScoreLabel.Text = $"Điểm cần đạt: {PassingScore:0.#} điểm";
        }

        private void SetupEventHandlers()
        {
            RetryButton.Clicked += OnRetryClicked;
            CloseButton.Clicked += OnCloseClicked;
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            Close();
            // ✅ SỬA: Pop ngược lại 2 trang để về LessonPage, rồi navigate tới ExamPage
            await Shell.Current.GoToAsync("../..");  // Pop 2 pages: ExamQuestionPage -> ExamPage -> LessonPage
            await Shell.Current.GoToAsync($"exampage?courseId={CourseId}"); // Navigate tới ExamPage fresh
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
            // ✅ SỬA: Pop ngược lại 2 trang để về LessonPage
            await Shell.Current.GoToAsync("../.."); // Pop 2 pages: ExamQuestionPage -> ExamPage -> LessonPage
        }
    }
}