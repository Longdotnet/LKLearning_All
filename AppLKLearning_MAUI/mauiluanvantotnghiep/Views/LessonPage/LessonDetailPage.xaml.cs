using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Views.LessonPage;

[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(CourseId), "courseId")]
public partial class LessonDetailPage : ContentPage
{
    int _lessonId;
    int _courseId;
    private string _currentSection = "";
    private bool _isAnimating = false;

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

    public LessonDetailPage()
    {
        InitializeComponent();
        BindingContext = new LessonDetailViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LessonDetailViewModel vm && LessonId > 0)
        {
            vm.CourseId = CourseId;
            vm.LessonId = LessonId;
            _ = vm.LoadLessonsAsync(LessonId);
        }
    }

    private void MainScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        // Custom scrollbar logic
        double contentHeight = MainScrollView.ContentSize.Height;
        double viewHeight = MainScrollView.Height;

        if (contentHeight <= viewHeight)
        {
            ThumbBar.TranslationY = 0;
        }
        else
        {
            double percent = e.ScrollY / (contentHeight - viewHeight);
            double trackHeight = viewHeight - ThumbBar.HeightRequest;
            double thumbY = percent * trackHeight;
            ThumbBar.TranslationY = thumbY;
        }

        // Chỉ update indicator khi thay đổi section (tránh spam animation)
        string newSection = "";
        string icon = "";
        int activeStep = 0;

        if (e.ScrollY >= contentHeight - viewHeight - 50) // Cuối trang
        {
            newSection = "quiz";
            icon = "🎯";
            activeStep = 4;
        }
        else if (e.ScrollY >= ContentSection.Y - 100)
        {
            newSection = "content";
            icon = "📚";
            activeStep = 3;
        }
        else if (e.ScrollY >= IntroductionSection.Y - 100)
        {
            newSection = "intro";
            icon = "📖";
            activeStep = 2;
        }
        else if (e.ScrollY >= TitleSection.Y - 50)
        {
            newSection = "title";
            icon = "📍";
            activeStep = 1;
        }
        else
        {
            newSection = "none";
        }

        // Chỉ update khi section thực sự thay đổi
        if (newSection != _currentSection && !_isAnimating)
        {
            _currentSection = newSection;
            
            if (newSection == "none")
            {
                _ = HideIndicator();
            }
            else
            {
                string text = newSection switch
                {
                    "title" => "Tiêu đề bài học",
                    "intro" => "Giới thiệu",
                    "content" => "Nội dung bài học",
                    "quiz" => "Làm Bài Tập",
                    _ => ""
                };
                _ = UpdateIndicatorSmooth(icon, text, activeStep);
            }
        }
    }

    private async Task HideIndicator()
    {
        if (!SectionIndicator.IsVisible) return;
        
        _isAnimating = true;
        await SectionIndicator.FadeTo(0, 200, Easing.CubicOut);
        SectionIndicator.IsVisible = false;
        _isAnimating = false;
    }

    private async Task UpdateIndicatorSmooth(string icon, string text, int activeStep)
    {
        _isAnimating = true;

        // Hiện indicator nếu đang ẩn
        if (!SectionIndicator.IsVisible)
        {
            SectionIndicator.IsVisible = true;
            SectionIndicator.Opacity = 0;
            await SectionIndicator.FadeTo(1, 300, Easing.CubicOut);
        }

        // Update content mà không animation
        IndicatorIcon.Text = icon;
        IndicatorLabel.Text = text;

        // Update progress dots với animation mượt
        var dots = new[] { Dot1, Dot2, Dot3, Dot4 };
        var tasks = new List<Task>();
        
        for (int i = 0; i < dots.Length; i++)
        {
            double targetOpacity = i < activeStep ? 1.0 : 0.3;
            if (Math.Abs(dots[i].Opacity - targetOpacity) > 0.1) // Chỉ animate khi có thay đổi đáng kể
            {
                tasks.Add(dots[i].FadeTo(targetOpacity, 200, Easing.CubicOut));
            }
        }

        // Subtle scale animation - chỉ 1 lần
        var scaleTask = Task.Run(async () =>
        {
            await SectionIndicator.ScaleTo(1.02, 150, Easing.CubicOut);
            await SectionIndicator.ScaleTo(1.0, 150, Easing.CubicOut);
        });

        await Task.WhenAll(tasks.Concat(new[] { scaleTask }));
        _isAnimating = false;
    }
}