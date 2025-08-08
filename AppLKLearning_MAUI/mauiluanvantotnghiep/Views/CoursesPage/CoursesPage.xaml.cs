using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using mauiluanvantotnghiep.ViewModels;
using mauiluanvantotnghiep.Models;

namespace mauiluanvantotnghiep.Views.CoursesPage;

public partial class CoursesPage : ContentPage
{
    private CoursePageViewModel _viewModel;

    public CoursesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Subscribe to scroll event
        if (BindingContext is CoursePageViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.ScrollToTopRequested += OnScrollToTopRequested;
            _viewModel.StartCarouselAutoScroll();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe and stop auto-scroll
        if (_viewModel != null)
        {
            _viewModel.ScrollToTopRequested -= OnScrollToTopRequested;
            _viewModel.StopCarouselAutoScroll();
        }
    }
    
    private async void OnScrollToTopRequested(object sender, EventArgs e)
    {
        // Scroll to top smoothly khi có k?t qu? search
        try
        {
            // Delay nh? ?? ??m b?o UI ?ã update xong
            await Task.Delay(100);
            
            if (MainScrollView != null)
            {
                await MainScrollView.ScrollToAsync(0, 0, true); // animated scroll to top
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Scroll to top error: {ex.Message}");
        }
    }
 
}