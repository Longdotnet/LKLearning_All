using System;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Behaviors
{
    public class WebViewScrollBehavior : Behavior<WebView>
    {
        protected override void OnAttachedTo(WebView bindable)
        {
            base.OnAttachedTo(bindable);
            
#if ANDROID
            bindable.Loaded += OnWebViewLoaded;
#elif IOS
            bindable.Loaded += OnWebViewLoaded;
#endif
        }

        protected override void OnDetachingFrom(WebView bindable)
        {
            base.OnDetachingFrom(bindable);
            
#if ANDROID || IOS
            bindable.Loaded -= OnWebViewLoaded;
#endif
        }

        private void OnWebViewLoaded(object sender, EventArgs e)
        {
            if (sender is WebView webView)
            {
#if ANDROID
                SetupAndroidScrolling(webView);
#elif IOS
                SetupIOSScrolling(webView);
#endif
            }
        }

#if ANDROID
        private void SetupAndroidScrolling(WebView webView)
        {
            var handler = webView.Handler as Microsoft.Maui.Handlers.WebViewHandler;
            if (handler?.PlatformView is Android.Webkit.WebView androidWebView)
            {
                // Disable zoom controls
                androidWebView.Settings.SetSupportZoom(false);
                androidWebView.Settings.BuiltInZoomControls = false;
                androidWebView.Settings.DisplayZoomControls = false;
                
                // Enable scrolling and scrollbars
                androidWebView.VerticalScrollBarEnabled = true;
                androidWebView.HorizontalScrollBarEnabled = false;
                
                // Enable touch scrolling
                androidWebView.ScrollBarStyle = Android.Views.ScrollbarStyles.InsideOverlay;
                
                // Additional settings for better scrolling
                androidWebView.Settings.JavaScriptEnabled = true;
                androidWebView.Settings.DomStorageEnabled = true;
                androidWebView.Settings.LoadWithOverviewMode = true;
                androidWebView.Settings.UseWideViewPort = false;
            }
        }
#endif

#if IOS
        private void SetupIOSScrolling(WebView webView)
        {
            var handler = webView.Handler as Microsoft.Maui.Handlers.WebViewHandler;
            if (handler?.PlatformView is WebKit.WKWebView wkWebView)
            {
                wkWebView.ScrollView.ScrollEnabled = true;
                wkWebView.ScrollView.Bounces = true;
                wkWebView.ScrollView.ShowsVerticalScrollIndicator = true;
                wkWebView.ScrollView.ShowsHorizontalScrollIndicator = false;
                wkWebView.ScrollView.AlwaysBounceVertical = false;
                wkWebView.ScrollView.AlwaysBounceHorizontal = false;
            }
        }
#endif
    }
}
