using Lib.AspNetCore.ServerSentEvents;
using sse_notify.Models;

var builder = WebApplication.CreateBuilder(args);


// ���U SSE �A��
builder.Services.AddServerSentEvents();
// ��� URL �]�t�� Guid ���o ClientId
builder.Services.AddSingleton<IServerSentEventsClientIdProvider, SseClientIdFromPathProvider>();
// �ۭq�@���~�� ServerSentEventsService �ι�@ IServerSentEventsService �����O�B�z�q��
// �ϥε{���w���Ѫ� AddServerSentEvents �X�R��k���U
builder.Services.AddServerSentEvents<SseNotifyService, InProcSseNotifyService>(options =>
{
    // �{���w���� KeepAlive �\��
    options.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;
    options.KeepaliveInterval = 15;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// �w�q SSE ��M���A�Ȥθ��ѤΫ��O�A�C�����ݪ����˾ާ@�����H�� GUID �ѧO
app.MapServerSentEvents<InProcSseNotifyService>("/sse/{regex(^[=0-9a-z].+)$)}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
