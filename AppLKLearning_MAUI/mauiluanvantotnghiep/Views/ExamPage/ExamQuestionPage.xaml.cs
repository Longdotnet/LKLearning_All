using mauiluanvantotnghiep.ViewModels;

namespace mauiluanvantotnghiep.Views.ExamPage;

[QueryProperty(nameof(ExamSetId), "examSetId")]
[QueryProperty(nameof(TimeLimitSec), "timeLimitSec")]
[QueryProperty(nameof(HistoryId), "historyId")]
[QueryProperty(nameof(PassingScore), "passingScore")]
[QueryProperty(nameof(CourseId), "courseId")]
public partial class ExamQuestionPage : ContentPage
{
    public int ExamSetId { get; set; }
    public int TimeLimitSec { get; set; }
    public int HistoryId { get; set; }
    public decimal PassingScore { get; set; }
    public int CourseId { get; set; }

    public ExamQuestionPage()
    {
        InitializeComponent();
        BindingContext = new ExamQuestionPageViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ExamQuestionPageViewModel vm && ExamSetId > 0)
        {
            // Set the properties in ViewModel
            vm.HistoryId = HistoryId;
            vm.PassingScore = PassingScore;
            vm.ExamSetId = ExamSetId;
            vm.CourseId = CourseId;
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Loading exam - ExamSetId: {ExamSetId}, HistoryId: {HistoryId}, PassingScore: {PassingScore}, CourseId: {CourseId}");
            
            await vm.LoadQuestionsAsync(ExamSetId);
            vm.StartCountdown(TimeLimitSec);
        }
    }
}