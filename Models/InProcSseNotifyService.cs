using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace sse_notify.Models;

public class InProcSseNotifyService : SseNotifyService, IServerSentEventsService
{
    private readonly IMemoryCache _cache;

    public InProcSseNotifyService(IOptions<ServerSentEventsServiceOptions<ServerSentEventsService>> options, 
        IMemoryCache cache) :
        base(options.ToBaseServerSentEventsServiceOptions<ServerSentEventsService>())
    {
        _cache = cache;
    }

    public override void Subscribe(Guid token, int timeoutSecs = 300)
    {
        var key = $"S:{token}";
        var semaphore = new SemaphoreSlim(0);
        var timeout = TimeSpan.FromSeconds(timeoutSecs);
        _cache.Set(key, semaphore, timeout);
        var task = Task.Factory.StartNew(async () =>
        {
            //wait for semaphore to be released
            if (!await semaphore.WaitAsync(TimeSpan.FromSeconds(timeoutSecs)))
                await SendEventAsync(token, "error", "Timeout");
            //try get response
            else if (!_cache.TryGetValue<string>($"R:{token}", out var res))
                await SendEventAsync(token, "error", "No response");
            else
                await SendEventAsync(token, "message", res);
        });

    }

    public async override Task Notify(Guid token, string message)
    {
        var key = $"S:{token}";
        if (!_cache.TryGetValue(key, out SemaphoreSlim semaphore))
            throw new ApplicationException("Token not found");
        _cache.Set("R:" + token.ToString(), message);
        semaphore.Release();
        _cache.Remove(key);
    }
}