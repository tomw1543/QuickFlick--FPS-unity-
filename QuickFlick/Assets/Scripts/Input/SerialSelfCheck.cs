using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace QuickFlick.Input
{
    // Recon only: logs raw serial lines. Reader runs on a background thread; Unity APIs stay on the main thread.
    public class SerialSelfCheck : MonoBehaviour
    {
        [SerializeField] string portOverride = "";
        [SerializeField] int baud = 9600;

        const float LogInterval = 0.2f; // max 5 logs/s

        readonly ConcurrentQueue<string> lines = new ConcurrentQueue<string>();
        Thread thread;
        volatile bool stop;
        SerialPort port;
        float nextLog;

        void OnEnable()
        {
            string name = portOverride;
            if (string.IsNullOrEmpty(name))
            {
                var names = SerialPort.GetPortNames();
                if (names.Length == 0) { Debug.LogWarning("[SerialSelfCheck] no serial ports found"); enabled = false; return; }
                name = names[0];
            }
            stop = false;
            thread = new Thread(() => Read(name)) { IsBackground = true };
            thread.Start();
        }

        void Read(string name)
        {
            try
            {
                port = new SerialPort(name, baud) { ReadTimeout = 200, DtrEnable = true };
                port.Open();
                while (!stop)
                {
                    try { lines.Enqueue(port.ReadLine().Trim()); }
                    catch (TimeoutException) { }
                }
            }
            catch (Exception e) { lines.Enqueue("ERROR " + e.Message); }
            finally
            {
                try { if (port != null && port.IsOpen) port.Close(); } catch { }
            }
        }

        void Update()
        {
            if (Time.unscaledTime < nextLog) return;
            string line = null;
            while (lines.TryDequeue(out var l)) line = l; // keep newest, drop older
            if (line == null) return;
            nextLog = Time.unscaledTime + LogInterval;
            Debug.Log($"[SerialSelfCheck] {line}");
        }

        void OnDisable()
        {
            stop = true;
            if (thread != null && !thread.Join(500)) Debug.LogWarning("[SerialSelfCheck] reader thread did not stop in time");
            thread = null;
        }

        void OnApplicationQuit() => OnDisable();
    }
}
