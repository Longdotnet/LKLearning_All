using mauiluanvantotnghiep.ViewModels;
using System.ComponentModel;

namespace mauiluanvantotnghiep.Views.StoryPage;

public partial class StoryPage : ContentPage
{
    private StoryViewModel _viewModel;
    private bool _isPlaying = false;

    public StoryPage()
    {
        InitializeComponent();
        _viewModel = (StoryViewModel)BindingContext;
    }

    private async void OnPlayPauseClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.CommandParameter is string url && !string.IsNullOrWhiteSpace(url))
            {
                if (!_isPlaying)
                {
                    // Play
                    btn.Text = "⏸️";
                    mediaPlayerUs.Source = url;
                    mediaPlayerUs.Play();
                    _isPlaying = true;
                }
                else
                {
                    // Pause
                    btn.Text = "▶️";
                    mediaPlayerUs.Pause();
                    _isPlaying = false;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể điều khiển audio: {ex.Message}", "OK");
            if (sender is Button btn)
            {
                btn.Text = "▶️";
                _isPlaying = false;
            }
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        try
        {
            mediaPlayerUs.Stop();
            _isPlaying = false;
            
            // Reset play button
            if (FindByName("playPauseBtn") is Button playBtn)
            {
                playBtn.Text = "▶️";
            }
        }
        catch (Exception ex)
        {
            // Ignore stop errors
        }
    }

    private void OnReplayClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button && !string.IsNullOrWhiteSpace(_viewModel?.ReadStoryURL))
            {
                mediaPlayerUs.Stop();
                mediaPlayerUs.Source = _viewModel.ReadStoryURL;
                mediaPlayerUs.Play();
                _isPlaying = true;
                
                // Update play button
                if (FindByName("playPauseBtn") is Button playBtn)
                {
                    playBtn.Text = "⏸️";
                }
            }
        }
        catch (Exception ex)
        {
            // Ignore replay errors
        }
    }

    private void OnRewind10Clicked(object sender, EventArgs e)
    {
        try
        {
            var currentPosition = mediaPlayerUs.Position;
            var newPosition = currentPosition.Subtract(TimeSpan.FromSeconds(10));
            if (newPosition < TimeSpan.Zero)
                newPosition = TimeSpan.Zero;
            
            mediaPlayerUs.SeekTo(newPosition);
        }
        catch (Exception ex)
        {
            // Ignore seek errors
        }
    }

    private void OnForward10Clicked(object sender, EventArgs e)
    {
        try
        {
            var currentPosition = mediaPlayerUs.Position;
            var newPosition = currentPosition.Add(TimeSpan.FromSeconds(10));
            var duration = mediaPlayerUs.Duration;
            
            if (duration > TimeSpan.Zero && newPosition > duration)
                newPosition = duration;
                
            mediaPlayerUs.SeekTo(newPosition);
        }
        catch (Exception ex)
        {
            // Ignore seek errors
        }
    }

    private void OnMediaEnded(object sender, EventArgs e)
    {
        _isPlaying = false;
        if (FindByName("playPauseBtn") is Button playBtn)
        {
            playBtn.Text = "▶️";
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (_viewModel != null)
        {
            _viewModel.ErrorMessage = "";
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        try
        {
            mediaPlayerUs?.Stop();
            _isPlaying = false;
        }
        catch (Exception)
        {
            // Ignore errors when stopping media
        }
    }
}