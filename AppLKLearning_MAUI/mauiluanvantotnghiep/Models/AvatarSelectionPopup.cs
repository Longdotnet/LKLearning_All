using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class AvatarSelectionPopup : Popup
    {
        public AvatarSelectionPopup(
            ObservableCollection<Avatar> avatars,
            int currentAvatarId,
            Action<Avatar> onSelected)
        {
            // 1. Không cho bấm ra ngoài popup
            CanBeDismissedByTappingOutsideOfPopup = false;

            // 2. Kích thước cố định cho popup
            Size = new Size(300, 430);

            // 3. Layout chính
            var layout = new VerticalStackLayout
            {
                Padding = new Thickness(12),
                Spacing = 12
            };

            // 4. Tiêu đề
            layout.Children.Add(new Label
            {
                Text = "Chọn Avatar",
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center
            });

            // 5. CollectionView dạng lưới 3 cột, cuộn dọc
            var collectionView = new CollectionView
            {
                ItemsSource = avatars,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 300,
                ItemsLayout = new GridItemsLayout(
                    span: 3,
                    orientation: ItemsLayoutOrientation.Vertical)
                {
                    HorizontalItemSpacing = 8,
                    VerticalItemSpacing = 8
                },
                // Không dùng SelectionChanged để tránh trường hợp click lại cùng 1 item không kích hoạt
                SelectionMode = SelectionMode.None,
                ItemTemplate = new DataTemplate(() =>
                {
                    // Khung ảnh
                    var frame = new Frame
                    {
                        Padding = 0,
                        CornerRadius = 8,
                        HasShadow = false,
                        BackgroundColor = Colors.LightGray,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    // Ảnh avatar
                    var image = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 80,
                        WidthRequest = 80
                    };
                    image.SetBinding(Image.SourceProperty, nameof(Avatar.UrlPath));
                    frame.Content = image;

                    // Thêm tap gesture lên toàn bộ frame
                    var tap = new TapGestureRecognizer();
                    tap.Command = new Command<Avatar>(avatar =>
                    {
                        onSelected?.Invoke(avatar);
                        Close();
                    });
                    tap.SetBinding(
                        TapGestureRecognizer.CommandParameterProperty,
                        new Binding("."));
                    frame.GestureRecognizers.Add(tap);

                    return frame;
                })
            };

            layout.Children.Add(collectionView);

            // 6. Nút Đóng
            var closeButton = new Button
            {
                Text = "Đóng",
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 100
            };
            closeButton.Clicked += (s, e) => Close();
            layout.Children.Add(closeButton);

            // 7. Bao khung popup
            Content = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 12,
                Padding = 0,
                Content = layout
            };

            // 8. Overlay mờ phía sau
            Color = Color.FromArgb("#80000000");
        }
    }
}
