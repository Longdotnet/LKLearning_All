// ViewModels/OptionItem.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Microsoft.Maui.Graphics;

namespace mauiluanvantotnghiep.ViewModels;

public partial class OptionItem : ObservableObject
{
    public string Text { get; }

    public OptionItem(string text)
    {
        Text = text;
    }

    bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return; // Only update if changed
            if (SetProperty(ref _isSelected, value) && value)
                OnSelected?.Invoke(this, EventArgs.Empty);
        }
    }

    Color _backgroundColor = Colors.Transparent;
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    public event EventHandler OnSelected;
}
