using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Converters
{
    public class VideoEmbedHtmlConverter : IValueConverter
    {
        // Regex lấy URL trong chuỗi HTML
        private static readonly Regex UrlRegex = new Regex(@"https:\/\/[^\s""'>]+", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var desc = value as string;
            string html;

            if (string.IsNullOrWhiteSpace(desc))
            {
                html = "<div></div>";
            }
            else
            {
                // Tìm URL đầu tiên trong chuỗi (nếu có)
                var match = UrlRegex.Match(desc);
                var url = match.Success ? match.Value : null;

                if (!string.IsNullOrEmpty(url) && url.StartsWith("https://player.vimeo.com/video/"))
                {

                    html = $@"
                    <html>
                    <head>
                    <style>
                    html, body {{
                        margin:0; padding:0; height:100%; width:100%;
                        background:transparent;
                    }}
                    .video-container {{
                        position:relative;
                        width:100vw;
                        max-width:100%;
                        height:100vh;
                        max-height:100%;
                        overflow:hidden;
                        background:transparent;
                    }}
                    .video-container iframe {{
                        position:absolute;
                        top:0; left:0;
                        width:100%;
                        height:100%;
                        border:0;
                    }}
                    </style>
                    </head>
                    <body>
                    <div class='video-container'>
                        <iframe src='{url}' frameborder='0' allowfullscreen allow='fullscreen'></iframe>
                    </div>
                    </body>
                    </html>";

                }
                else if (!string.IsNullOrEmpty(url) && (url.Contains("youtube.com") || url.Contains("youtu.be")))
                {
                    html = $@"
                        <html>
                        <head>
                        <style>
                        html, body {{
                            margin:0; padding:0; height:100%; width:100%;
                            background:transparent;
                        }}
                        .video-container {{
                            position:relative;
                            width:100vw;
                            max-width:100%;
                            height:100vh;
                            max-height:100%;
                            overflow:hidden;
                            background:transparent;
                        }}
                        .video-container iframe {{
                            position:absolute;
                            top:0; left:0;
                            width:100%;
                            height:100%;
                            border:0;
                        }}
                        </style>
                        </head>
                        <body>
                        <div class='video-container'>
                        <iframe width='100%' height='360' src='{url.Replace("watch?v=", "embed/")}' frameborder='0' allowfullscreen allow='fullscreen'></iframe></div>
                        </body>
                        </html>";

                }
                else
                {
                    // Không phải video URL, hiển thị như cũ
                    html = $"<html><body>{desc}</body></html>";
                }
            }

            return new HtmlWebViewSource { Html = html };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
