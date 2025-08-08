using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Converters
{
    public class HtmlWrapperConverter : IValueConverter
    {
        // Cache để tránh tạo lại HTML source liên tục
        private static readonly ConcurrentDictionary<string, HtmlWebViewSource> _cache = new();
        
        // Regex lấy URL trong chuỗi HTML (tương tự VideoEmbedHtmlConverter)
        private static readonly Regex UrlRegex = new Regex(@"https:\/\/[^\s""'>]+", RegexOptions.Compiled);
        
        // CSS tối ưu để tránh layout shifts
        private const string Template = @"
<html>
<head>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, user-scalable=no""/>
  <style>
    html, body {{ 
        max-width: 100vw; 
        overflow-x: hidden; 
        margin: 0; 
        padding: 10px; 
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        line-height: 1.4;
        word-wrap: break-word;
        background: transparent;
    }}
    p {{ 
        text-align: justify; 
        margin: 0 0 10px 0; 
        padding: 0; 
    }}
    img, video, audio {{ 
        max-width: 100%; 
        height: auto; 
        display: block; 
        margin: 10px auto; 
    }}
    iframe {{ 
        max-width: 100%; 
        border: none;
    }}
    .video-container {{
        position: relative;
        width: 100%;
        max-width: 100%;
        height: 250px;
        margin: 15px 0;
        overflow: hidden;
        background: transparent;
        border-radius: 8px;
    }}
    .video-container iframe {{
        position: absolute;
        top: 0; 
        left: 0;
        width: 100%;
        height: 100%;
        border: 0;
    }}
    /* Ngăn content resize liên tục */
    * {{
        box-sizing: border-box;
    }}
    /* Ẩn scrollbar để tránh flicker */
    ::-webkit-scrollbar {{
        display: none;
    }}
  </style>
</head>
<body>
  {0}
</body>
</html>";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var desc = value as string ?? string.Empty;
            
            // Kiểm tra cache trước
            if (_cache.TryGetValue(desc, out var cachedSource))
            {
                return cachedSource;
            }
            
            string html;

            if (string.IsNullOrWhiteSpace(desc))
            {
                html = string.Format(Template, "<div></div>");
            }
            else
            {
                // Tìm URL đầu tiên trong chuỗi (nếu có) - tương tự VideoEmbedHtmlConverter
                var match = UrlRegex.Match(desc);
                var url = match.Success ? match.Value : null;
                
                string processedContent = desc;

                if (!string.IsNullOrEmpty(url) && url.StartsWith("https://player.vimeo.com/video/"))
                {
                    // Xử lý Vimeo video
                    var videoHtml = $@"
                    <div class='video-container'>
                        <iframe src='{url}' frameborder='0' allowfullscreen></iframe>
                    </div>";
                    
                    // Thay thế URL bằng video embed và giữ lại text
                    processedContent = desc.Replace(url, videoHtml);
                }
                else if (!string.IsNullOrEmpty(url) && (url.Contains("youtube.com") || url.Contains("youtu.be")))
                {
                    // Xử lý YouTube video
                    var embedUrl = url.Replace("watch?v=", "embed/");
                    var videoHtml = $@"
                    <div class='video-container'>
                        <iframe src='{embedUrl}' frameborder='0' allowfullscreen></iframe>
                    </div>";
                    
                    // Thay thế URL bằng video embed và giữ lại text
                    processedContent = desc.Replace(url, videoHtml);
                }

                html = string.Format(Template, processedContent);
            }
            
            // Tạo mới và cache
            var htmlSource = new HtmlWebViewSource
            {
                Html = html
            };
            
            // Cache với giới hạn để tránh memory leak
            if (_cache.Count < 100) // Giới hạn 100 items
            {
                _cache.TryAdd(desc, htmlSource);
            }
            
            return htmlSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        // Method để clear cache nếu cần
        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
