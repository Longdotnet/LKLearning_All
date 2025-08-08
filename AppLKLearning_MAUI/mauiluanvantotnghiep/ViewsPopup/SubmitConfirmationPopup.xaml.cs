using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.ViewsPopup
{
    public partial class SubmitConfirmationPopup : Popup
    {
        public int TotalQuestions { get; }
        public int AnsweredQuestions { get; }
        public string TimeRemaining { get; }
        public bool IsConfirmed { get; private set; } = false;

        public SubmitConfirmationPopup(int totalQuestions, int answeredQuestions, string timeRemaining)
        {
            InitializeComponent();
            
            TotalQuestions = totalQuestions;
            AnsweredQuestions = answeredQuestions;
            TimeRemaining = timeRemaining;
            
            InitializeContent();
            SetupEventHandlers();
        }

        private void InitializeContent()
        {
            QuestionStatsLabel.Text = $"Câu đã trả lời: {AnsweredQuestions}/{TotalQuestions}";
            TimeRemainingLabel.Text = $"Thời gian còn lại: {TimeRemaining}";
        }

        private void SetupEventHandlers()
        {
            CancelButton.Clicked += OnCancelClicked;
            SubmitButton.Clicked += OnSubmitClicked;
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            IsConfirmed = false;
            Close(IsConfirmed);
        }

        private void OnSubmitClicked(object sender, EventArgs e)
        {
            IsConfirmed = true;
            Close(IsConfirmed);
        }
    }
}