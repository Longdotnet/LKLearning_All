using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels.Popup
{
    public partial class AlertLessonExamViewModel : ObservableObject
    {
        private readonly int _courseId;
        private readonly Action _closePopup;

        public AlertLessonExamViewModel(int courseId, Action closePopup)
        {
            _courseId = courseId;
            _closePopup = closePopup;
        }

        [RelayCommand]
        private async Task StartExamAsync()
        {
            _closePopup?.Invoke(); // Đóng popup trước
            await Shell.Current.GoToAsync($"exampage?courseId={_courseId}");
        }
    }
}