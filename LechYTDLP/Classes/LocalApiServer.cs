using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LechYTDLP.Classes
{
    using LechYTDLP.Services;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading;

    public class RequestData
    {
        public string Url { get; set; } = string.Empty;
        public string ExtensionVersion { get; set; } = string.Empty;
        public string ExtensionBrowser { get; set; } = string.Empty;
    }

    public class LocalApiServer
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts = new();

        public event Action<RequestData>? DownloadRequested;

        public LocalApiServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:3781/");
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                Task.Run(ListenLoop);
            }
            catch (HttpListenerException ex)
            {
                // This often happens when the user doesn't have permission to listen on the specified port.
                LogService.Add(App.LocalizationService.GetString("BrowserExtWontWork", ex.Message), LogTag.ApiServer);
                return;
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }

        private async Task ListenLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(context));
                }
                catch { }
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            // CORS headerları
            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-Extension-Version, X-Extension-Browser");

            // Preflight request
            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            if (req.HttpMethod == "GET" && req.Url!.AbsolutePath == "/ping")
            {
                res.StatusCode = 200;
                await WriteResponse(res, "pong");
                return;
            }

            if (req.HttpMethod == "POST" && req.Url!.AbsolutePath == "/download")
            {
                using var reader = new StreamReader(req.InputStream);
                var body = await reader.ReadToEndAsync();

                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

                if (json != null && json.TryGetValue("url", out var url))
                {
                    //"X-Extension-Version": chrome.runtime.getManifest().version,
                    //"X-Extension-Browser": navigator.userAgent.includes("Firefox") ? "Firefox" : "Chrome"


                    DownloadRequested?.Invoke(new RequestData
                    {
                        Url = url,
                        ExtensionVersion = req.Headers.Get("X-Extension-Version") ?? "",
                        ExtensionBrowser = req.Headers.Get("X-Extension-Browser") ?? "",
                    });
                    res.StatusCode = 200;
                    await WriteResponse(res, "ok");
                    return;
                }
            }

            res.StatusCode = 404;
            await WriteResponse(res, "not found");
        }

        private async Task WriteResponse(HttpListenerResponse res, string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            res.ContentLength64 = buffer.Length;
            await res.OutputStream.WriteAsync(buffer);
            res.Close();
        }
    }

}
