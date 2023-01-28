using Lib.AspNetCore.ServerSentEvents;
using sse_notify.Models;

var builder = WebApplication.CreateBuilder(args);


// 註冊 SSE 服務
builder.Services.AddServerSentEvents();
// 改由 URL 包含的 Guid 取得 ClientId
builder.Services.AddSingleton<IServerSentEventsClientIdProvider, SseClientIdFromPathProvider>();
// 自訂一個繼承 ServerSentEventsService 及實作 IServerSentEventsService 的類別處理通知
// 使用程式庫提供的 AddServerSentEvents 擴充方法註冊
builder.Services.AddServerSentEvents<SseNotifyService, InProcSseNotifyService>(options =>
{
    // 程式庫提供 KeepAlive 功能
    options.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;
    options.KeepaliveInterval = 15;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 定義 SSE 對映的服務及路由及型別，每次等待的掃瞄操作產生隨機 GUID 識別
app.MapServerSentEvents<InProcSseNotifyService>("/sse/{regex(^[=0-9a-z].+)$)}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
