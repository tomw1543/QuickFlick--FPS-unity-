using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace QuickFlick.Net
{
    // Plain C#, no Unity API. Background thread owns the socket; main thread only
    // calls TryDequeue / IsConnected / LastError / Start / Stop.
    public class RelayClient : IDisposable
    {
        readonly string _url;
        readonly int _maxQueue;
        readonly double _reconnectMax;
        readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        readonly object _lock = new object();

        Thread _thread;
        CancellationTokenSource _cts;
        ClientWebSocket _ws;
        volatile bool _connected;
        volatile string _lastError;
        volatile bool _replaced;

        public bool IsConnected => _connected;
        public string LastError => _lastError;
        public bool Replaced => _replaced;

        public RelayClient(string socketUrl, int maxQueue = 512, double reconnectMaxSeconds = 10)
        {
            _url = socketUrl;
            _maxQueue = maxQueue < 1 ? 1 : maxQueue;
            _reconnectMax = reconnectMaxSeconds < 0.5 ? 0.5 : reconnectMaxSeconds;
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_thread != null && _thread.IsAlive) return;
                try { if (_cts != null) _cts.Dispose(); } catch (Exception) { }
                _replaced = false;
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                _thread = new Thread(() => Run(token)) { IsBackground = true, Name = "RelayClient" };
                _thread.Start();
            }
        }

        public void Stop()
        {
            Thread t; CancellationTokenSource cts; ClientWebSocket ws;
            lock (_lock)
            {
                t = _thread; cts = _cts; ws = _ws;
                _thread = null; _cts = null;
            }
            if (t == null) return;
            try { cts.Cancel(); } catch (Exception) { }
            try { if (ws != null) ws.Abort(); } catch (Exception) { }
            try { t.Join(1000); } catch (Exception) { }
            try { cts.Dispose(); } catch (Exception) { }
            _connected = false;
        }

        public void Dispose() => Stop();

        public bool TryDequeue(out string message) => _queue.TryDequeue(out message);

        // Background thread. Never throws.
        void Run(CancellationToken token)
        {
            double backoff = 0.5;
            var buf = new byte[4096];
            while (!token.IsCancellationRequested)
            {
                ClientWebSocket ws = null;
                try
                {
                    ws = new ClientWebSocket();
                    lock (_lock) { _ws = ws; }
                    ws.ConnectAsync(new Uri(_url), token).GetAwaiter().GetResult();
                    _connected = true;
                    _lastError = null;
                    backoff = 0.5;
                    Receive(ws, buf, token);
                }
                catch (Exception e)
                {
                    if (!token.IsCancellationRequested) _lastError = e.GetType().Name + ": " + e.Message;
                }
                finally
                {
                    _connected = false;
                    lock (_lock) { if (ReferenceEquals(_ws, ws)) _ws = null; }
                    try { if (ws != null) ws.Dispose(); } catch (Exception) { }
                }

                if (token.IsCancellationRequested || _replaced) break;
                try { token.WaitHandle.WaitOne(TimeSpan.FromSeconds(backoff)); } catch (Exception) { break; }
                backoff = Math.Min(backoff * 2, _reconnectMax);
            }
        }

        void Receive(ClientWebSocket ws, byte[] buf, CancellationToken token)
        {
            var ms = new MemoryStream();
            bool discarding = false;
            while (!token.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                var r = ws.ReceiveAsync(new ArraySegment<byte>(buf), token).GetAwaiter().GetResult();
                if (r.MessageType == WebSocketMessageType.Close)
                {
                    if ((int?)r.CloseStatus == 4001)
                    {
                        _lastError = "Replaced by another game in this room";
                        _replaced = true;
                    }
                    else _lastError = "closed by relay (" + (int?)r.CloseStatus + ")";
                    return;
                }
                if (r.MessageType == WebSocketMessageType.Binary) { ms.SetLength(0); SkipBinary(ws, r, buf, token); continue; }
                if (!discarding)
                {
                    ms.Write(buf, 0, r.Count);
                    if (ms.Length > 65536) { discarding = true; ms.SetLength(0); } // oversized; relay caps at 1 KB anyway
                }
                if (!r.EndOfMessage) continue;
                if (discarding) { discarding = false; continue; }
                Enqueue(Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length));
                ms.SetLength(0);
            }
        }

        // Drain remaining fragments of a binary message so framing stays aligned.
        static void SkipBinary(ClientWebSocket ws, WebSocketReceiveResult first, byte[] buf, CancellationToken token)
        {
            var r = first;
            while (!r.EndOfMessage && !token.IsCancellationRequested)
                r = ws.ReceiveAsync(new ArraySegment<byte>(buf), token).GetAwaiter().GetResult();
        }

        void Enqueue(string msg)
        {
            _queue.Enqueue(msg);
            while (_queue.Count > _maxQueue && _queue.TryDequeue(out _)) { }
        }
    }
}
