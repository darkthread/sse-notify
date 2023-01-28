using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Options;

namespace sse_notify.Models
{
    // https://tpeczek.github.io/Lib.AspNetCore.ServerSentEvents/articles/getting-started.html
    public abstract class SseNotifyService : ServerSentEventsService, IServerSentEventsService
    {

        public SseNotifyService(IOptions<ServerSentEventsServiceOptions<ServerSentEventsService>> options) :
            base(options.ToBaseServerSentEventsServiceOptions<ServerSentEventsService>())
        {
        }

        public abstract void Subscribe(Guid token, int timeoutSecs = 300);

        public async Task SendEventAsync(Guid token, string type, string message)
        {
            var client = this.GetClients().SingleOrDefault(o =>
                o.Id == token);
            if (client != null)
            {
                await client.SendEventAsync(new ServerSentEvent()
                {
                    Type = type,
                    Data = new List<string> { message }
                });
            }
        }

        public abstract Task Notify(Guid token, string message);
    }
}
