using CommunityToolkit.Maui.Views;
using mauiluanvantotnghiep.ViewModels;
using mauiluanvantotnghiep.Views.VideoPopupPage;
using Microsoft.Maui.Controls; 
namespace mauiluanvantotnghiep.Views.QuestionPage;

[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(CourseId), "courseId")]
public partial class QuestionPage : ContentPage
{
    int _lessonId;
    int _courseId;
    
    public int LessonId
    {
        get => _lessonId;
        set
        {
            _lessonId = value;
        }
    }
    
    public int CourseId
    {
        get => _courseId;
        set
        {
            _courseId = value;
        }
    }

    public QuestionPage()
    {
        InitializeComponent();
        BindingContext = new QuestionPageViewModel();
        
        // Subscribe to ViewModel navigation events
        if (BindingContext is QuestionPageViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is QuestionPageViewModel vm && LessonId > 0)
        {
            vm.CourseId = CourseId;
            vm.LessonId = LessonId;
            _ = vm.LoadQuestionsAsync(LessonId);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe to prevent memory leaks
        if (BindingContext is QuestionPageViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private async void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Scroll to top when question changes
        if (e.PropertyName == nameof(QuestionPageViewModel.CurrentIndex) || 
            e.PropertyName == nameof(QuestionPageViewModel.CurrentQuestion))
        {
            // Small delay to ensure UI is updated
            await Task.Delay(100);
            await ScrollToTopAsync();
        }
    }

    private async Task ScrollToTopAsync()
    {
        try
        {
            await MainScrollView.ScrollToAsync(0, 0, true);
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Error scrolling to top: {ex.Message}");
        }
    }
    private void OnFullScreenClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string questionDescription)
        {
            // Convert the questionDescription to HTML if needed (like your converter does)
            var converter = new mauiluanvantotnghiep.Converters.VideoEmbedHtmlConverter();
            var htmlWebViewSource = converter.Convert(questionDescription, typeof(HtmlWebViewSource), null, null) as HtmlWebViewSource;
            if (htmlWebViewSource != null)
            {
                var popup = new VideoFullScreenPopup(htmlWebViewSource);
                this.ShowPopup(popup);
            }
            
        }
    }


}
