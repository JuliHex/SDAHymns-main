using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDAHymns.Core.Services;

public interface IRemoteControlHandler
{
    Task<string> GetStatusJsonAsync();
    void NextVerse();
    void PreviousVerse();
    Task LoadHymnAsync(int number, string category);
    void ToggleBlackScreen();
}

public class RemoteControlServer : IDisposable
{
    private HttpListener? _listener;
    private readonly IRemoteControlHandler _handler;
    private bool _isRunning;
    private readonly int _port;

    public RemoteControlServer(IRemoteControlHandler handler, int port = 4546)
    {
        _handler = handler;
        _port = port;
    }

    public void Start()
    {
        if (_isRunning) return;

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{_port}/");
            _listener.Start();
            _isRunning = true;

            Task.Run(ListenLoop);
            Console.WriteLine($"Remote Control Server started on port {_port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start Remote Control Server: {ex.Message}");
        }
    }

    private async Task ListenLoop()
    {
        while (_isRunning && _listener != null)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (HttpListenerException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ListenLoop: {ex.Message}");
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
                return;
            }

            string result = "OK";
            var path = request.Url?.AbsolutePath.ToLowerInvariant() ?? "";

            switch (path)
            {
                case "/status":
                    result = await _handler.GetStatusJsonAsync();
                    response.ContentType = "application/json";
                    break;
                case "/next":
                    _handler.NextVerse();
                    break;
                case "/prev":
                    _handler.PreviousVerse();
                    break;
                case "/blank":
                    _handler.ToggleBlackScreen();
                    break;
                case "/load":
                    if (int.TryParse(request.QueryString["number"], out int num))
                    {
                        string cat = request.QueryString["category"] ?? "crestine";
                        await _handler.LoadHymnAsync(num, cat);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        result = "Missing number";
                    }
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    result = "Not Found";
                    break;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(result);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            byte[] buffer = Encoding.UTF8.GetBytes(ex.Message);
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        finally
        {
            response.Close();
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
    }

    public void Dispose()
    {
        Stop();
    }
}
