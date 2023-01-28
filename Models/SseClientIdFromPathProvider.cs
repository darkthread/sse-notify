using System.Text.RegularExpressions;
using Lib.AspNetCore.ServerSentEvents;

namespace sse_notify.Models
{
    public class SseClientIdFromPathProvider : IServerSentEventsClientIdProvider
    {
        public Guid AcquireClientId(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var m = Regex.Match(path, @"(?i)/(?<g>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})$");
            if (m.Success) return Guid.Parse(m.Groups["g"].Value);
            return Guid.NewGuid();
        }

        public void ReleaseClientId(Guid clientId, HttpContext context)
        {
           
        }
    }
}
