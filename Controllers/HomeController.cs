using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using sse_notify.Models;

namespace sse_notify.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SseNotifyService _notifySvc;

    public HomeController(ILogger<HomeController> logger, SseNotifyService notifySvc)
    {
        _logger = logger;
        _notifySvc = notifySvc;
    }

    public IActionResult Index()
    {
        var token = Guid.NewGuid();
        ViewBag.Token = token;
        return View();
    }

    (string url, string dataUri) GenQRCode(Guid token, int timeoutSecs)
    {
        _notifySvc.Subscribe(token, timeoutSecs);
        var url = String.Concat(Request.Scheme, "://", Request.Host, Request.PathBase, $"/Home/Notify/{token}");
        //create QRCode
        var qrCode = new QRCodeGenerator();
        var qrCodeData = qrCode.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(qrCodeData).GetGraphic(4);
        var dataUri = $"data:image/png;base64,{Convert.ToBase64String(png)}";
        return (url, dataUri);
    }

    
    [Route("Home/QRCode/{token}")]
    public IActionResult QRCode(Guid token)
    {
        var timeoutSecs = 30;
        var qrCode = GenQRCode(token, timeoutSecs);
        ViewBag.QRCodePng = qrCode.dataUri;
        ViewBag.Url = qrCode.url;
        ViewBag.TimeoutSecs = timeoutSecs;
        return View();
    }
    

    [Route("Home/Notify/{token}")]
    public IActionResult Notify(Guid token, string message)
    {
        _notifySvc.Notify(token, message ?? "Scanned");
        return Content(@"<html>
<head>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<head><body><h1>Webpage is notified from SSE</h1></body>
</html>", "text/html");
    }

    public IActionResult Test()
    {
        var timeoutSecs = 10;
        var token = Guid.NewGuid();
        var qrCode = GenQRCode(token, timeoutSecs);
        ViewBag.QRCodePng = qrCode.dataUri;
        ViewBag.Url = qrCode.url;
        ViewBag.TimeoutSecs = timeoutSecs;
        ViewBag.Token = token;
        return View();
    }

    public IActionResult Succ()
    {
        return View();
    }
}
