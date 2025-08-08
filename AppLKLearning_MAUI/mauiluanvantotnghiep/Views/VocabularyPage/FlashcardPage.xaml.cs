using System;
using mauiluanvantotnghiep.ViewModels.VocabularyViewModel;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Views.VocabularyPage
{
    [QueryProperty(nameof(CategoryId), "categoryId")]
    public partial class FlashcardPage : ContentPage
    {
        // Lấy ViewModel từ BindingContext
        FlashcardViewModel Vm => BindingContext as FlashcardViewModel;

        // Flag để định hướng swipe: true = trái, false = phải
        bool _swipeLeft = true;
        public FlashcardPage()
        {
            InitializeComponent();
            BindingContext = new FlashcardViewModel();
        }

        // Đón nhận categoryId từ QueryParams và gọi load data
        private string categoryId;
        public string CategoryId
        {
            get => categoryId;
            set
            {
                categoryId = value;
                if (Vm != null && int.TryParse(value, out var catId))
                    Vm.LoadVocabulariesCommand.Execute(catId);
                else if (Vm != null)
                    Vm.ErrorMessage = "Tham số categoryId không hợp lệ.";
            }
        }

        // Nút Flip Card — animation + gọi FlipCommand trong VM
        private async void OnFlipButtonClicked(object sender, EventArgs e)
        {
            // 1) Quay 90° để làm mặt hiện tại ẩn dần
            await CardContainer.RotateYTo(5, 500, Easing.CubicIn);

            // 2) Toggle mặt front/back
            Vm.FlipCommand.Execute(null);

            // 3) Đặt thẳng container ở -90° (mặt mới đang ẩn phía sau)
            CardContainer.RotationY = -5;

            // 4) Quay về 0° để mặt mới hiện lên
            await CardContainer.RotateYTo(0, 500, Easing.CubicOut);
        }

        async void OnRemoveButtonClicked(object sender, EventArgs e)
        {
            // 1) Tính offset theo flag
            var w = CardContainer.Width;
            var h = CardContainer.Height;
            // Swipe chéo lên + sang trái hoặc phải
            var offX = _swipeLeft ? -w : w;
            var offY = -h;

            // 2) Chạy animation swipe
            await CardContainer.TranslateTo(offX, offY, 300, Easing.CubicIn);

            // 3) Gọi lệnh Remove trong VM để xóa card
            Vm.RemoveCommand.Execute(null);

            // 4) Đổi hướng swipe cho lần sau
            _swipeLeft = !_swipeLeft;

            // 5) Reset vị trí CardContainer để sẵn sàng cho card mới
            CardContainer.TranslationX = 0;
            CardContainer.TranslationY = 0;
        }
        // Handle play audio UK
        private void OnPlayClickedItemLisstUk(object sender, EventArgs e)
        {
            if (sender is ImageButton btn &&
                btn.CommandParameter is string url &&
                !string.IsNullOrWhiteSpace(url))
            {
                mediaPlayerlistUk.Source = url;
                mediaPlayerlistUk.Play();
            }
        }

        // Handle play audio US
        private void OnPlayClickedItemLisstUs(object sender, EventArgs e)
        {
            if (sender is ImageButton btn &&
                btn.CommandParameter is string url &&
                !string.IsNullOrWhiteSpace(url))
            {
                mediaPlayerlistUs.Source = url;
                mediaPlayerlistUs.Play();
            }
        }

     

    }
}
