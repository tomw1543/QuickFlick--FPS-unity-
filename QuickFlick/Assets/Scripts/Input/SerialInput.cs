using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace QuickFlick.Input
{
    // shortcut: no zero-pose calibration, assumes flat-ish rest; upgrade when drift bites
    // shortcut: fire on raw delta-g only, no gesture classification; upgrade if phantom fires
    public class SerialInput : MonoBehaviour, IControllerInput
    {
        [SerializeField] string portOverride = "";
        [SerializeField] int baud = 9600;
        [SerializeField] float deadzoneG = 0.1f;
        [SerializeField] float fullTiltG = 0.7f;
        [SerializeField] float spikeThresholdG = 0.375f;
        [SerializeField] float firePulseSeconds = 0.1f;
        [SerializeField] bool invertX = false;
        [SerializeField] bool invertY = false;
        [SerializeField] bool swapXY = true;
        [SerializeField] float spikeCooldownSeconds = 0.15f;

        SerialPort _port;
        Thread _thread;
        volatile bool _stop;
        volatile bool _failed;
        readonly ConcurrentQueue<string> _lines = new();

        Vector2 _tilt;
        Vector3 _prev;
        bool _hasPrev;
        float _rightFireUntil, _cooldownUntil;

        public Vector2 LeftStick => _tilt;
        public Vector2 RightStick => _tilt;
        public bool LeftFire => false;
        public bool RightFire => Time.time < _rightFireUntil;
        public bool IsOpen => _port != null && _port.IsOpen && !_failed;
        public string PortName => _port?.PortName;

        void OnEnable()
        {
            var name = portOverride;
            if (string.IsNullOrEmpty(name))
            {
                var names = SerialPort.GetPortNames();
                if (names.Length == 0) { Debug.LogWarning("[SerialInput] no serial ports found"); return; }
                name = names[0];
            }

            try
            {
                _port = new SerialPort(name, baud) { ReadTimeout = 500, NewLine = "\n" };
                _port.Open();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SerialInput] open {name} failed: {e.Message}");
                Close();
                return;
            }

            _stop = false;
            _failed = false;
            _thread = new Thread(ReadLoop) { IsBackground = true, Name = "SerialInput" };
            _thread.Start();
        }

        // Background thread: no Unity APIs here.
        void ReadLoop()
        {
            while (!_stop)
            {
                try { _lines.Enqueue(_port.ReadLine().Trim()); }
                catch (TimeoutException) { }
                catch (Exception) { if (!_stop) _failed = true; break; }
            }
        }

        void OnDisable() => Close();
        void OnApplicationQuit() => Close();

        void Close()
        {
            _stop = true;
            try { if (_thread != null && _thread.IsAlive) _thread.Join(500); }
            finally
            {
                _thread = null;
                var p = _port;
                _port = null;
                try { if (p != null && p.IsOpen) p.Close(); } catch (Exception) { }
            }
        }

        void Update()
        {
            bool got = false;
            Vector3 a = default;
            while (_lines.TryDequeue(out var line))
            {
                if (!SerialProtocol.TryParseSample(line, out a)) continue;
                got = true;
                if (_hasPrev && Time.time >= _cooldownUntil)
                {
                    if (Mathf.Abs(a.x - _prev.x) > spikeThresholdG)
                    {
                        _rightFireUntil = Time.time + firePulseSeconds;
                        _cooldownUntil = Time.time + spikeCooldownSeconds;
                    }
                }
                _prev = a;
                _hasPrev = true;
            }
            if (!got) return;

            float x = swapXY ? a.y : a.x;
            float y = swapXY ? a.x : a.y;
            if (invertX) x = -x;
            if (invertY) y = -y;
            _tilt = new Vector2(Shape(x), Shape(y));
        }

        float Shape(float v) => Mathf.Abs(v) < deadzoneG ? 0f : Mathf.Clamp(v / fullTiltG, -1f, 1f);
    }
}
