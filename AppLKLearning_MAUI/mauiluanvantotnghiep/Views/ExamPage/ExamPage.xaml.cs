using mauiluanvantotnghiep.ViewModels.ExamViewModel;

namespace mauiluanvantotnghiep.Views.ExamPage;

[QueryProperty(nameof(CourseId), "courseId")]
public partial class ExamPage : ContentPage
{
    public int CourseId { get; set; }
    private readonly ExamPageViewModel _viewModel;

    public ExamPage()
    {
        InitializeComponent();
        _viewModel = new ExamPageViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (CourseId > 0)
            await _viewModel.LoadExamAsync(CourseId);
    }
}