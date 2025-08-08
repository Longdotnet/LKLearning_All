// ViewModels/BlankItem.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

public class BlankItem : ObservableObject
{
    public string CorrectAnswer { get; }
    public BlankItem(string correct) => CorrectAnswer = correct;

    string _userText;
    public string UserText
    {
        get => _userText;
        set => SetProperty(ref _userText, value);
    }

    Color _borderColor = Colors.Transparent;
    public Color BorderColor
    {
        get => _borderColor;
        set => SetProperty(ref _borderColor, value);
    }
}
