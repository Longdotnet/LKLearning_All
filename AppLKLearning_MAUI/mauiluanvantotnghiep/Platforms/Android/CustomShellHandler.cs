using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ANDROID
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Handlers;
using AndroidView = Android.Views.View;
using AndroidViewGroup = Android.Views.ViewGroup;
#endif

namespace mauiluanvantotnghiep.Platforms.Android
{
#if ANDROID
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

            // TẮT HOÀN TOÀN TINTING cho các tab chính
            bottomView.ItemIconTintList = null;
        }
    }
#endif
}
