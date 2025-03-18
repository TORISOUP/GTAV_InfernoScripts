using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inferno.Utilities.Http
{
    public sealed class SimpleJsonHttpServer<T> : IDisposable
    {
        private readonly HttpListener _httpListener;
        private CancellationTokenSource _cts = new();
        public Action<T> OnRequestReceived { get; set; }
        private bool _isDisposed;

        public SimpleJsonHttpServer(int port)
        {
            _httpListener = new HttpListener();

            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        public void Start()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(SimpleJsonHttpServer<T>));
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            Task.Run(() => _httpListener.Start());
            _ = HandleRequestsAsync(_cts.Token);
        }

        public void Stop()
        {
            if (_isDisposed) return;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _httpListener.Stop();
        }

        private async Task HandleRequestsAsync(CancellationToken ct)
        {
            var currentSyncContext = SynchronizationContext.Current;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync().ConfigureAwait(false);
                    var request = context.Request;
                    var response = context.Response;
                    try
                    {
                        if (request.HttpMethod == "POST")
                        {
                            using (var reader = new StreamReader(request.InputStream))
                            {
                                var json = await reader.ReadToEndAsync();
                                T v = default;
                                try
                                {
                                    v = JsonConvert.DeserializeObject<T>(json);
                                }
                                catch (Exception)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Close();
                                    continue;
                                }

                                if (currentSyncContext != null)
                                {
                                    currentSyncContext.Post(_ => OnRequestReceived?.Invoke(v), null);
                                }
                                else
                                {
                                    OnRequestReceived?.Invoke(v);
                                }
                            }

                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.Close();
                        }
                        else
                        {
                            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                            response.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        response.Close();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Stop();
            ((IDisposable)_httpListener)?.Dispose();
        }
    }
}