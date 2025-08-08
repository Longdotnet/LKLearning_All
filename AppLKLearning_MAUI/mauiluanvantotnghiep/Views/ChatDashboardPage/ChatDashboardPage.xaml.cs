using System;
using Microsoft.Maui.Controls;
using mauiluanvantotnghiep.ViewModels.ChatDashboardViewModel;
using System.Collections.Specialized;

namespace mauiluanvantotnghiep.Views.ChatDashboardPage;

public partial class ChatDashboardPage : ContentPage
{
    private ChatDashboardViewModel _viewModel;

    public ChatDashboardPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ChatDashboardViewModel viewModel)
        {
            _viewModel = viewModel;
            
            // Subscribe to Messages collection changes for auto-scroll
            viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
            
            // Auto-create conversation when page appears if not exists
            if (viewModel.Conversation == null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await viewModel.CreateConversationCommand.ExecuteAsync(null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Create conversation error: {ex.Message}");
                    }
                });
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from events
        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
        }
    }

    // Auto-scroll to bottom when new messages arrive
    private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Small delay to ensure UI is updated
                    await Task.Delay(300);
                    
                    // Scroll to last item in CollectionView
                    if (MessagesCollectionView != null && _viewModel?.Messages?.Count > 0)
                    {
                        var lastMessage = _viewModel.Messages.Last();
                        MessagesCollectionView.ScrollTo(lastMessage, animate: true);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Auto-scroll error: {ex.Message}");
                }
            });
        }
    }
}