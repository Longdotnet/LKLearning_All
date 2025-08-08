using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels.AppConfig;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace mauiluanvantotnghiep.ViewModels.ChatDashboardViewModel
{
    public partial class ChatDashboardViewModel : ObservableObject
    {
        [ObservableProperty] private Conversation conversation;
        [ObservableProperty] private string statusMessage;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string newMessage = string.Empty;
        [ObservableProperty] private bool isConnected;
        [ObservableProperty] private bool canSendMessage;

        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        public IAsyncRelayCommand CreateConversationCommand { get; }
        public IAsyncRelayCommand SendMessageCommand { get; }
        public IAsyncRelayCommand LoadMessagesCommand { get; }
        public IAsyncRelayCommand RefreshConnectionCommand { get; } // THÊM command mới

        private readonly HttpClient _httpClient;
        private HubConnection _hubConnection;
        private int _currentUserId;
        private bool _isSignalRInitialized = false; // THÊM flag để track trạng thái

        public ChatDashboardViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
            
            CreateConversationCommand = new AsyncRelayCommand(CreateConversationAsync);
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessageFunc);
            LoadMessagesCommand = new AsyncRelayCommand(LoadMessagesAsync);
            RefreshConnectionCommand = new AsyncRelayCommand(RefreshSignalRConnectionAsync); // THÊM command mới

            // KHÔNG khởi tạo SignalR ngay trong constructor
            // Sẽ khởi tạo khi cần thiết
        }

        // THÊM method để refresh SignalR connection
        private async Task RefreshSignalRConnectionAsync()
        {
            try
            {
                StatusMessage = "Đang làm mới kết nối(...)";
                
                // Đóng connection cũ nếu có
                if (_hubConnection != null)
                {
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                    _isSignalRInitialized = false;
                }

                // Khởi tạo lại
                await InitializeSignalRAsync();
                
                // Rejoin conversation nếu có
                if (Conversation != null && IsConnected)
                {
                    await JoinConversationGroup(Conversation.ConversationID);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi làm mới kết nối: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[SignalR] Refresh error: {ex}");
            }
        }

        // THÊM method để đảm bảo SignalR được khởi tạo
        private async Task EnsureSignalRInitializedAsync()
        {
            if (!_isSignalRInitialized || _hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                System.Diagnostics.Debug.WriteLine("[SignalR] Initializing SignalR connection...");
                await InitializeSignalRAsync();
            }
        }

        private bool CanSendMessageFunc()
        {
            return CanSendMessage && !string.IsNullOrWhiteSpace(NewMessage);
        }

        private int ExtractUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                // Debug: in ra tất cả claims để kiểm tra
                System.Diagnostics.Debug.WriteLine("=== JWT Claims ===");
                foreach (var claim in jwtToken.Claims)
                {
                    System.Diagnostics.Debug.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
                }
                
                // Lấy claim theo ClaimTypes.NameIdentifier (giống như bên API)
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found UserId: {userId}");
                    return userId;
                }

                // Fallback: thử các claim khác
                var subClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub");
                if (subClaim != null && int.TryParse(subClaim.Value, out int subUserId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found UserId via sub: {subUserId}");
                    return subUserId;
                }

                var nameIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid");
                if (nameIdClaim != null && int.TryParse(nameIdClaim.Value, out int nameIdUserId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found UserId via nameid: {nameIdUserId}");
                    return nameIdUserId;
                }

                System.Diagnostics.Debug.WriteLine("User ID not found in any claim");
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting user ID from token: {ex.Message}");
                return 0;
            }
        }

        private string ExtractUserNameFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
                if (emailClaim != null)
                {
                    return emailClaim.Value;
                }
                
                return "Bạn";
            }
            catch
            {
                return "Bạn";
            }
        }

        private async Task InitializeSignalRAsync()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("[SignalR] No auth token found");
                    return;
                }

                // Extract current user ID from token using JWT library
                _currentUserId = ExtractUserIdFromToken(token);
                if (_currentUserId == 0)
                {
                    StatusMessage = "Không thể lấy thông tin user từ token";
                    System.Diagnostics.Debug.WriteLine("[SignalR] Could not extract user ID from token");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[SignalR] Current User ID: {_currentUserId}");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{AppConfig.AppConfig.BaseUrl}/chatHub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                        options.HttpMessageHandlerFactory = _ => handler;
                        options.CloseTimeout = TimeSpan.FromSeconds(30); // THÊM timeout
                        options.SkipNegotiation = false; // THÊM để đảm bảo negotiation
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                    .Build();

                // Handle incoming messages with better logging
                _hubConnection.On<int, string, string, DateTime>("ReceiveMessage", async (senderId, senderName, content, sentAt) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] ReceiveMessage Event Triggered!");
                    System.Diagnostics.Debug.WriteLine($"[SignalR] SenderId: {senderId}, SenderName: {senderName}");
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Content: {content}, SentAt: {sentAt}");
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Current User ID: {_currentUserId}");
                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            var newMessage = new Message
                            {
                                SenderID = senderId,
                                SenderName = senderName,
                                Content = content,
                                SentAt = sentAt,
                                ConversationID = Conversation?.ConversationID ?? 0,
                                IsRead = senderId == _currentUserId
                            };
                            
                            // Check if message already exists to prevent duplicates
                            var existingMessage = Messages.FirstOrDefault(m => 
                                m.SenderID == senderId && 
                                m.Content == content && 
                                Math.Abs((m.SentAt - sentAt).TotalSeconds) < 2);
                            
                            if (existingMessage == null)
                            {
                                Messages.Add(newMessage);
                                System.Diagnostics.Debug.WriteLine($"[SignalR] Message added to UI from {senderName}");
                                
                                if (senderId != _currentUserId)
                                {
                                    StatusMessage = $"Tin nhắn mới từ {senderName}";
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[SignalR] Duplicate message ignored");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SignalR] Error processing received message: {ex.Message}");
                        }
                    });
                });

                // Better connection state handling
                _hubConnection.Reconnected += async (connectionId) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Reconnected with ID: {connectionId}");
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        IsConnected = true;
                        StatusMessage = "Đã kết nối lại SignalR";
                    });
                    
                    // Rejoin conversation nếu có
                    if (Conversation != null)
                    {
                        await JoinConversationGroup(Conversation.ConversationID);
                    }
                };

                _hubConnection.Closed += async (error) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Connection closed: {error?.Message}");
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        IsConnected = false;
                        _isSignalRInitialized = false; // RESET flag
                        StatusMessage = "Mất kết nối SignalR";
                    });
                };

                _hubConnection.Reconnecting += async (error) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Reconnecting: {error?.Message}");
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        StatusMessage = "Đang kết nối lại...";
                    });
                };

                System.Diagnostics.Debug.WriteLine("[SignalR] Starting connection...");
                await _hubConnection.StartAsync();
                
                IsConnected = true;
                _isSignalRInitialized = true; // SET flag
                StatusMessage = "Đã kết nối SignalR thành công";
                System.Diagnostics.Debug.WriteLine("[SignalR] Connection started successfully");
            }
            catch (Exception ex)
            {
                _isSignalRInitialized = false; // RESET flag khi có lỗi
                StatusMessage = $"Lỗi kết nối SignalR: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[SignalR] Error: {ex}");
            }
        }

        private async Task JoinConversationGroup(int conversationId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.SendAsync("JoinConversation", conversationId);
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Successfully joined conversation group: {conversationId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] Error joining conversation group {conversationId}: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR] Cannot join conversation group - connection state: {_hubConnection?.State}");
            }
        }

        private async Task CreateConversationAsync()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsLoading = true;
                StatusMessage = "Đang kiểm tra cuộc trò chuyện...";
                Conversation = null;
                Messages.Clear();
            });

            try
            {
                // ĐẢM BẢO SignalR được khởi tạo trước
                await EnsureSignalRInitializedAsync();

                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusMessage = "Bạn chưa đăng nhập.";
                    });
                    return;
                }

                if (_currentUserId == 0)
                {
                    _currentUserId = ExtractUserIdFromToken(token);
                    if (_currentUserId == 0)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            StatusMessage = "Không thể lấy thông tin user từ token";
                        });
                        return;
                    }
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/Conversations/user-create";
                var response = await _httpClient.PostAsync(apiUrl, null);
                var json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Create Conversation Response: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (json.Contains("Không có admin nào đang online"))
                        {
                            StatusMessage = "Hiện tại không có admin online. Bạn vẫn có thể gửi tin nhắn và admin sẽ phản hồi khi online.";
                            CanSendMessage = true; // THAY ĐỔI: vẫn cho phép gửi tin nhắn
                        }
                        else
                        {
                            StatusMessage = "Lỗi tạo hội thoại: " + json;
                        }
                    });
                    return;
                }

                var conversationResult = JsonSerializer.Deserialize<Conversation>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Conversation = conversationResult;
                    if (conversationResult != null)
                    {
                        var timeSinceCreated = DateTime.Now - conversationResult.CreatedAt;
                        if (timeSinceCreated.TotalMinutes < 1)
                        {
                            StatusMessage = $"Đã kết nối với {conversationResult.AdminName}!";
                        }
                        else
                        {
                            StatusMessage = $"Tiếp tục cuộc trò chuyện với {conversationResult.AdminName}";
                        }
                    }
                    CanSendMessage = true;
                });

                // Join SignalR group và load messages
                if (conversationResult != null)
                {
                    await JoinConversationGroup(conversationResult.ConversationID);
                    await LoadMessagesAsync();
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusMessage = "Lỗi: " + ex.Message;
                });
                System.Diagnostics.Debug.WriteLine($"Create Conversation Error: {ex}");
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private async Task LoadMessagesAsync()
        {
            if (Conversation == null) return;

            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/Messages/conversation/{Conversation.ConversationID}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Load Messages Response: {json}");
                    
                    var messages = JsonSerializer.Deserialize<List<Message>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Messages.Clear();
                        if (messages != null)
                        {
                            foreach (var message in messages)
                            {
                                if (message.SenderID == _currentUserId)
                                {
                                    message.SenderName = "Bạn";
                                }
                                Messages.Add(message);
                            }
                        }
                    });
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Load Messages Error: {errorJson}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tải tin nhắn: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Load Messages Exception: {ex}");
            }
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMessage) || Conversation == null)
                return;

            var messageContent = NewMessage.Trim();
            
            try
            {
                // ĐẢM BẢO SignalR được khởi tạo trước khi gửi
                await EnsureSignalRInitializedAsync();

                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return;

                if (_currentUserId == 0)
                {
                    _currentUserId = ExtractUserIdFromToken(token);
                    if (_currentUserId == 0)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            StatusMessage = "Không thể lấy thông tin user từ token";
                        });
                        return;
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NewMessage = string.Empty;
                    StatusMessage = IsConnected ? "Đang gửi tin nhắn..." : "Gửi tin nhắn (admin sẽ nhận khi online)";
                });

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var messageDto = new 
                {
                    conversationID = Conversation.ConversationID,
                    senderID = _currentUserId,
                    content = messageContent,
                    sentAt = DateTime.Now,
                    isRead = false
                };

                var json = JsonSerializer.Serialize(messageDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/Messages/send";
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusMessage = $"Lỗi gửi tin nhắn: {errorJson}";
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StatusMessage = IsConnected ? "Tin nhắn đã gửi" : "Tin nhắn đã gửi (admin sẽ nhận khi online)";
                    });
                    System.Diagnostics.Debug.WriteLine($"[API] Message sent successfully: {messageContent}");
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusMessage = $"Lỗi gửi tin nhắn: {ex.Message}";
                });
                System.Diagnostics.Debug.WriteLine($"Send Message Exception: {ex}");
            }
        }

        partial void OnNewMessageChanged(string value)
        {
            SendMessageCommand.NotifyCanExecuteChanged();
        }

        partial void OnCanSendMessageChanged(bool value)
        {
            SendMessageCommand.NotifyCanExecuteChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                if (Conversation != null)
                {
                    try
                    {
                        await _hubConnection.SendAsync("LeaveConversation", Conversation.ConversationID);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error leaving conversation: {ex.Message}");
                    }
                }
                
                await _hubConnection.DisposeAsync();
            }
            _httpClient?.Dispose();
        }

        public int CurrentUserId => _currentUserId;
    }
}
