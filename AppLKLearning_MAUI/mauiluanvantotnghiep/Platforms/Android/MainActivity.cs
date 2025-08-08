using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Firebase;
using Firebase.Messaging;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Core.Platforms.Android;



namespace mauiluanvantotnghiep
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Khởi tạo Firebase
            try
            {
                FirebaseApp.InitializeApp(this);
                Android.Util.Log.Debug("FCM", "Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("FCM", $"Firebase initialization failed: {ex.Message}");
            }

            // Subscribe topic "all" bằng API gốc
            FirebaseMessaging.Instance
              .SubscribeToTopic("all")
              .AddOnCompleteListener(new OnCompleteListener(task =>
              {
                  if (task.IsSuccessful)
                      Android.Util.Log.Debug("FCM", "Subscribed to topic 'all'");
                  else
                      Android.Util.Log.Error("FCM", $"Subscribe failed: {task.Exception}");
              }));
            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();
        }



   

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private static void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        private void CreateNotificationChannelIfNeeded()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateNotificationChannel();
            }
        }

        private void CreateNotificationChannel()
        {
            var channelId = $"{PackageName}.general";

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = channelId;
        }
    }
}

