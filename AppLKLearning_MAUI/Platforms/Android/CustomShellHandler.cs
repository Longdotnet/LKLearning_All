#if ANDROID
using Microsoft.Maui.Handlers;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Platform;

namespace mauiluanvantotnghiep.Platforms.Android
{
    public class CustomShellHandler : Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer
    {
        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new CustomTabBarAppearanceTracker(this, shellItem);
        }
    }

    public class CustomTabBarAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        public CustomTabBarAppearanceTracker(IShellContext shellContext, ShellItem shellItem) : base(shellContext, shellItem)
        {
        }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);
            
            // T?T HOÀN TOÀN TINTING - gi? màu nguyên b?n c?a icon
            bottomView.ItemIconTintList = null;
        }
    }
}
#endif