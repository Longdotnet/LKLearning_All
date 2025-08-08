using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using WEBAPI.Hubs;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Services.Bannerbear;
using WEBAPI.Services.VnPay;
using WEBAPI.Services.ZaloPay;


var builder = WebApplication.CreateBuilder(args);


// Cấu hình chuỗi kết nối
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LuanvantienganhContext>(options =>
    options.UseSqlServer(connectionString));

// Cấu hình HttpClient
builder.Services.AddHttpClient();

// Đăng ký ZaloPayService
builder.Services.AddScoped<ZaloPayService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();


// Cấu hình JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
var key = Encoding.ASCII.GetBytes(jwtSecret);

//cấu hình cho BannerbearService
// Đăng ký HttpClient cho BannerbearService
builder.Services
       .AddHttpClient<IBannerbearService, BannerbearService>(client =>
       {
           client.BaseAddress = new Uri("https://api.bannerbear.com/v2/");
       });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
    // Thêm đoạn này để lấy token từ query string cho SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSwaggerGen(c =>
{
    // Định nghĩa bảo mật với scheme là Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer' theo sau là token của bạn. Ví dụ: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Yêu cầu rằng tất cả các endpoint đều cần bảo mật theo scheme trên
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
// Thêm CORS policy cho phép mọi origin hoặc chỉ origin của bạn
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder
            .WithOrigins(
                "https://localhost:7159",    // Cho phép local FE (nếu cần)
                "http://localhost:5036",     // Cho phép local FE (nếu cần)
                "http://localhost:7159",     // Cho phép local FE (nếu cần)
                "http://testwebapi.somee.com" // Cho phép domain API (nếu cần test từ chính API)
                                              // Thêm domain FE thực tế nếu có
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IOnlineUserTracker, OnlineUserTracker>();
var app = builder.Build();
// Sử dụng CORS cho toàn bộ API
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// Phải gọi UseAuthentication trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub"); // Thêm dòng này trước app.Run()
app.Run();
