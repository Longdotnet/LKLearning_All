using CommunityToolkit.Maui.Views;
using mauiluanvantotnghiep.Models;
using System.Net.Http;

namespace mauiluanvantotnghiep.ViewsPopup
{
    public partial class CertificateViewPopup : Popup
    {
        public CertificateResponse Certificate { get; }

        public CertificateViewPopup(CertificateResponse certificate)
        {
            InitializeComponent();
            Certificate = certificate;
            BindingContext = certificate;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        private async void OnDownloadClicked(object sender, EventArgs e)
        {
            try
            {
                if (Certificate == null || string.IsNullOrEmpty(Certificate.ImageUrl))
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", 
                        "Không có chứng chỉ để tải về", "OK");
                    return;
                }

                // Show loading state - giống như trong ExamPassedPopup
                var downloadButton = sender as Button;
                if (downloadButton != null)
                {
                    downloadButton.Text = "Đang tải...";
                    downloadButton.IsEnabled = false;
                }

                // Gọi hàm tải xuống - copy từ ExamPassedPopup
                await DownloadCertificateToGalleryAsync(Certificate.ImageUrl);

                // Reset button state
                if (downloadButton != null)
                {
                    downloadButton.Text = "📥 Tải xuống";
                    downloadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Download Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", 
                    $"Lỗi khi tải về: {ex.Message}", "OK");

                // Reset button state
                var downloadButton = sender as Button;
                if (downloadButton != null)
                {
                    downloadButton.Text = "📥 Tải xuống";
                    downloadButton.IsEnabled = true;
                }
            }
        }

        // Copy nguyên hàm từ ExamPassedPopup
        private async Task DownloadCertificateToGalleryAsync(string imageUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Downloading certificate from: {imageUrl}");
                
                // Download the certificate image
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Downloaded {imageBytes.Length} bytes");
                
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
                
                await Application.Current.MainPage.DisplayAlert("Thành công", 
                    "Chứng chỉ đã được lưu vào thư viện ảnh!", "OK");
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] HTTP Error: {httpEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi mạng", 
                    "Không thể tải chứng chỉ. Kiểm tra kết nối mạng.", "OK");
            }
            catch (TaskCanceledException timeoutEx)
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Timeout Error: {timeoutEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", 
                    "Tải chứng chỉ quá lâu. Vui lòng thử lại.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Download Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", 
                    $"Không thể tải chứng chỉ: {ex.Message}", "OK");
            }
        }

#if ANDROID
        private async Task SaveToAndroidGalleryAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                // Copy từ ExamPassedPopup
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
                        System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Saved to Android Gallery: {uri}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Android Gallery Error: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Saved to Downloads: {filePath}");
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
                        System.Diagnostics.Debug.WriteLine("[CertificateViewPopup] Saved to iOS Photo Library");
                        tcs.SetResult(true);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] iOS Photo Library Error: {error.LocalizedDescription}");
                        tcs.SetException(new Exception(error.LocalizedDescription));
                    }
                });
                
                await tcs.Task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] iOS Save Error: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] Saved to Documents: {filePath}");
                
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
                System.Diagnostics.Debug.WriteLine($"[CertificateViewPopup] FileSystem Save Error: {ex.Message}");
                throw;
            }
        }
    }
}