using CommunityToolkit.Maui.Views;
using mauiluanvantotnghiep.Models;
using System.Diagnostics;

namespace mauiluanvantotnghiep.ViewsPopup
{
    public partial class ExamPassedPopup : Popup
    {
        public Certificate Certificate { get; }
        public decimal TotalScore { get; }
        public decimal PassingScore { get; }
        public int CourseId { get; }

        public ExamPassedPopup(Certificate certificate, decimal totalScore, decimal passingScore, int courseId)
        {
            InitializeComponent();
            
            Certificate = certificate;
            TotalScore = totalScore;
            PassingScore = passingScore;
            CourseId = courseId;
            
            Debug.WriteLine($"[ExamPassedPopup] Initializing with ImageUrl: {certificate?.ImageUrl}");
            
            InitializeContent();
            SetupEventHandlers();
        }

        private void InitializeContent()
        {
            try
            {
                // Set score labels with proper formatting
                ScoreLabel.Text = $"Điểm số: {TotalScore:F1} điểm";
                PassingScoreLabel.Text = $"Điểm cần đạt: {PassingScore:F1} điểm";
                
                Debug.WriteLine($"[ExamPassedPopup] Score labels set - Total: {TotalScore:F1}, Passing: {PassingScore:F1}");

                // Set certificate image
                if (Certificate != null && !string.IsNullOrEmpty(Certificate.ImageUrl))
                {
                    Debug.WriteLine($"[ExamPassedPopup] Setting certificate image: {Certificate.ImageUrl}");
                    CertificateImage.Source = ImageSource.FromUri(new Uri(Certificate.ImageUrl));
                    
                    // Add error handling for image loading
                    CertificateImage.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CertificateImage.IsLoading))
                        {
                            Debug.WriteLine($"[ExamPassedPopup] Image loading: {CertificateImage.IsLoading}");
                        }
                    };
                }
                else
                {
                    Debug.WriteLine("[ExamPassedPopup] No certificate image URL provided");
                    CertificateImage.Source = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] InitializeContent Error: {ex.Message}");
            }
        }

        private void SetupEventHandlers()
        {
            DownloadButton.Clicked += OnDownloadClicked;
            ShareButton.Clicked += OnShareClicked; // ✅ THÊM: Event handler cho Share button
            CloseButton.Clicked += OnCloseClicked;
        }

        private async void OnDownloadClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[ExamPassedPopup] Download button clicked");
                
                if (Certificate != null && !string.IsNullOrEmpty(Certificate.ImageUrl))
                {
                    Debug.WriteLine($"[ExamPassedPopup] Starting download for: {Certificate.ImageUrl}");
                    
                    // Show loading state
                    DownloadButton.Text = "Đang tải...";
                    DownloadButton.IsEnabled = false;
                    
                    await DownloadCertificateToGalleryAsync(Certificate.ImageUrl);
                    
                    // Reset button state
                    DownloadButton.Text = "Tải về";
                    DownloadButton.IsEnabled = true;
                }
                else
                {
                    Debug.WriteLine("[ExamPassedPopup] No certificate URL to download");
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có chứng chỉ để tải về", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] OnDownloadClicked Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi tải về: {ex.Message}", "OK");
                
                // Reset button state
                DownloadButton.Text = "Tải về";
                DownloadButton.IsEnabled = true;
            }
        }

        // ✅ THÊM: Share button handler
        private async void OnShareClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[ExamPassedPopup] Share button clicked");
                
                if (Certificate != null && !string.IsNullOrEmpty(Certificate.ImageUrl))
                {
                    Debug.WriteLine($"[ExamPassedPopup] Starting share for: {Certificate.ImageUrl}");
                    
                    // Show loading state
                    ShareButton.Text = "Đang tải...";
                    ShareButton.IsEnabled = false;
                    
                    await ShareCertificateAsync(Certificate.ImageUrl);
                    
                    // Reset button state
                    ShareButton.Text = "Chia sẻ";
                    ShareButton.IsEnabled = true;
                }
                else
                {
                    Debug.WriteLine("[ExamPassedPopup] No certificate URL to share");
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có chứng chỉ để chia sẻ", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] OnShareClicked Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi chia sẻ: {ex.Message}", "OK");
                
                // Reset button state
                ShareButton.Text = "Chia sẻ";
                ShareButton.IsEnabled = true;
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("[ExamPassedPopup] Close button clicked");
                Close();
                
                // Navigate back to lesson page
                await Shell.Current.GoToAsync("../.."); // Pop 2 pages: ExamQuestionPage -> ExamPage -> LessonPage
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] OnCloseClicked Error: {ex.Message}");
                Close(); // Ensure popup closes even if navigation fails
            }
        }

        // ✅ THÊM: Share certificate method
        private async Task ShareCertificateAsync(string imageUrl)
        {
            try
            {
                Debug.WriteLine($"[ExamPassedPopup] Sharing certificate from: {imageUrl}");
                
                // Download the certificate image to temp folder
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                Debug.WriteLine($"[ExamPassedPopup] Downloaded {imageBytes.Length} bytes for sharing");
                
                var fileName = $"Certificate_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                
                // Save to temp/cache directory for sharing
                var tempPath = FileSystem.Current.CacheDirectory;
                var filePath = Path.Combine(tempPath, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);
                Debug.WriteLine($"[ExamPassedPopup] Saved to temp for sharing: {filePath}");

                // Create share request
                var request = new ShareFileRequest
                {
                    Title = "Chia sẻ chứng chỉ",
                    File = new ShareFile(filePath),
                    PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS ? new Rect(0, 20, 0, 0) : Rect.Zero
                };

                // Add text content for sharing
                var textRequest = new ShareTextRequest
                {
                    Title = "Chia sẻ chứng chỉ",
                    Text = $"🎉 Tôi vừa hoàn thành khóa học và đạt được chứng chỉ! 🎉\n\n" +
                           $"📊 Điểm số: {TotalScore:F1}/{PassingScore:F1}\n" +
                           $"✅ Trạng thái: Đạt yêu cầu\n\n" +
                           $"Cảm ơn các bạn đã ủng hộ! 💪",
                    Subject = "Chứng chỉ hoàn thành khóa học"
                };

                // Try to share file first, fallback to text if needed
                try
                {
                    await Share.RequestAsync(request);
                    Debug.WriteLine("[ExamPassedPopup] File share completed");
                }
                catch (Exception fileShareEx)
                {
                    Debug.WriteLine($"[ExamPassedPopup] File share failed: {fileShareEx.Message}");
                    Debug.WriteLine("[ExamPassedPopup] Falling back to text share");
                    
                    // Fallback to text sharing
                    await Share.RequestAsync(textRequest);
                    Debug.WriteLine("[ExamPassedPopup] Text share completed");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"[ExamPassedPopup] HTTP Error: {httpEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi mạng", "Không thể tải chứng chỉ để chia sẻ. Kiểm tra kết nối mạng.", "OK");
            }
            catch (TaskCanceledException timeoutEx)
            {
                Debug.WriteLine($"[ExamPassedPopup] Timeout Error: {timeoutEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Tải chứng chỉ quá lâu. Vui lòng thử lại.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] Share Error: {ex.Message}");
                
                // Fallback: Share text only without image
                try
                {
                    var fallbackRequest = new ShareTextRequest
                    {
                        Title = "Chia sẻ thành tích",
                        Text = $"🎉 Tôi vừa hoàn thành khóa học và đạt được {TotalScore:F1} điểm! 🎉\n" +
                               $"✅ Đã vượt qua điểm cần đạt: {PassingScore:F1} điểm\n\n" +
                               $"Cảm ơn các bạn đã ủng hộ! 💪"
                    };
                    
                    await Share.RequestAsync(fallbackRequest);
                    Debug.WriteLine("[ExamPassedPopup] Fallback text share completed");
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"[ExamPassedPopup] Fallback share failed: {fallbackEx.Message}");
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể chia sẻ chứng chỉ", "OK");
                }
            }
        }

        // Keep existing download methods unchanged...
        private async Task DownloadCertificateToGalleryAsync(string imageUrl)
        {
            try
            {
                Debug.WriteLine($"[ExamPassedPopup] Downloading certificate from: {imageUrl}");
                
                // Download the certificate image
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                Debug.WriteLine($"[ExamPassedPopup] Downloaded {imageBytes.Length} bytes");
                
                var fileName = $"Certificate_{DateTime.Now:yyyyMMdd_HHmmss}.png";

#if ANDROID
                // ✅ ANDROID: Tải về trực tiếp vào thư viện ảnh
                await SaveToAndroidGalleryAsync(imageBytes, fileName);
#elif IOS
                // ✅ iOS: Tải về vào Photo Library
                await SaveToiOSPhotoLibraryAsync(imageBytes, fileName);
#else
                // ✅ FALLBACK: Dùng FileSystem API cho platform khác
                await SaveWithFileSystemAsync(imageBytes, fileName);
#endif
                
                await Application.Current.MainPage.DisplayAlert("Thành công", "Chứng chỉ đã được lưu vào thư viện ảnh!", "OK");
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"[ExamPassedPopup] HTTP Error: {httpEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi mạng", "Không thể tải chứng chỉ. Kiểm tra kết nối mạng.", "OK");
            }
            catch (TaskCanceledException timeoutEx)
            {
                Debug.WriteLine($"[ExamPassedPopup] Timeout Error: {timeoutEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Tải chứng chỉ quá lâu. Vui lòng thử lại.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] Download Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải chứng chỉ: {ex.Message}", "OK");
            }
        }

#if ANDROID
        private async Task SaveToAndroidGalleryAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                // Sử dụng MediaStore để lưu vào thư viện ảnh Android
                var contentResolver = Platform.CurrentActivity?.ContentResolver ?? 
                                    Android.App.Application.Context.ContentResolver;

                var contentValues = new Android.Content.ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, 
                    Android.OS.Environment.DirectoryPictures + "/Certificates");

                var uri = contentResolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                
                if (uri != null)
                {
                    using var outputStream = contentResolver.OpenOutputStream(uri);
                    if (outputStream != null)
                    {
                        await outputStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                        await outputStream.FlushAsync();
                        Debug.WriteLine($"[ExamPassedPopup] Saved to Android Gallery: {uri}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] Android Gallery Error: {ex.Message}");
                // Fallback to Downloads folder
                await SaveToAndroidDownloadsAsync(imageBytes, fileName);
            }
        }

        private async Task SaveToAndroidDownloadsAsync(byte[] imageBytes, string fileName)
        {
            var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes);
            Debug.WriteLine($"[ExamPassedPopup] Saved to Downloads: {filePath}");
        }
#endif

#if IOS
        private async Task SaveToiOSPhotoLibraryAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                var image = UIKit.UIImage.LoadFromData(Foundation.NSData.FromArray(imageBytes));
                
                var tcs = new TaskCompletionSource<bool>();
                
                image.SaveToPhotosAlbum((uiImage, error) =>
                {
                    if (error == null)
                    {
                        Debug.WriteLine("[ExamPassedPopup] Saved to iOS Photo Library");
                        tcs.SetResult(true);
                    }
                    else
                    {
                        Debug.WriteLine($"[ExamPassedPopup] iOS Photo Library Error: {error.LocalizedDescription}");
                        tcs.SetException(new Exception(error.LocalizedDescription));
                    }
                });
                
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] iOS Save Error: {ex.Message}");
                // Fallback to Documents
                await SaveWithFileSystemAsync(imageBytes, fileName);
            }
        }
#endif

        private async Task SaveWithFileSystemAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                var documentsPath = FileSystem.Current.AppDataDirectory;
                var filePath = Path.Combine(documentsPath, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);
                Debug.WriteLine($"[ExamPassedPopup] Saved to Documents: {filePath}");
                
                // Try to use Share API as additional option
                var request = new ShareFileRequest
                {
                    Title = "Lưu chứng chỉ vào thư viện ảnh",
                    File = new ShareFile(filePath)
                };

                await Share.RequestAsync(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExamPassedPopup] FileSystem Save Error: {ex.Message}");
                throw;
            }
        }
    }
}