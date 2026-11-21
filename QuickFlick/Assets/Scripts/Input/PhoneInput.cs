using System.Collections.Generic;
using QuickFlick.Net;
using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Input
{
    // Phone as a controller. Always drains the relay queue (even when not the selected input)
    // so a push spike is never lost and IsAvailable stays accurate. Never throws.
    public class PhoneInput : MonoBehaviour, IControllerInput
    {
        RelayClient client;
        readonly List<PhoneReading> samples = new List<PhoneReading>(4);
        readonly PushDetector push = new PushDetector(
            Settings.PhonePushThreshold, Settings.PhonePushRelease, Settings.PhonePushCooldownSeconds);

        bool needNeutral = true;
        float alpha0, beta0;
        int lastRecenter;
        bool haveRecenter;

        double lastSampleRealtime = -100;
        float rightFireUntil, aimFreezeUntil;
        Vector2 stick;
        string lastLoggedError;

        // msgs/s window
        int msgCount;
        float msgWindowStart;

        // debug state
        public string ConfigError { get; private set; }
        public bool PhonePresent { get; private set; }
        public float MessagesPerSecond { get; private set; }
        public float LastAlpha { get; private set; }
        public float LastBeta { get; private set; }
        public float LastGamma { get; private set; }
        public float DeltaAlpha { get; private set; }
        public float DeltaBeta { get; private set; }
        public float LastAy { get; private set; }
        public float RecentPeakAy { get; private set; }
        public int PushCount { get; private set; }
        public bool Replaced => client != null && client.Replaced;
        public bool RelayConnected => client != null && client.IsConnected;
        public string RelayError => client?.LastError;

        public Vector2 LeftStick => stick;
        public Vector2 RightStick => stick;
        public bool LeftFire => false;
        public bool RightFire => Time.time < rightFireUntil;

        public bool IsAvailable =>
            !Replaced && RelayConnected && Time.realtimeSinceStartupAsDouble - lastSampleRealtime <= Settings.PhoneTimeoutSeconds;

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += StopClient;
#endif
            if (!EnvConfig.TryLoad(out var url, out var room, out var error))
            {
                ConfigError = error;
                Debug.LogWarning("[Phone] disabled: " + error);
                return;
            }
            ConfigError = null;
            client = new RelayClient(RelayUrls.GameSocketUrl(url, room), Settings.PhoneRelayQueueMax, Settings.PhoneReconnectMaxSeconds);
            client.Start();
            JoinQrWriter.Write(RelayUrls.JoinUrl(url, room));
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= StopClient;
#endif
            StopClient();
        }

        void OnApplicationQuit() => StopClient();

        public void RestartRelay()
        {
            if (client == null) return;
            client.Stop();
            client.Start();
            lastLoggedError = null;
        }

        void StopClient()
        {
            if (client == null) return;
            client.Stop();
            client = null;
        }

        void Update()
        {
            if (client == null) return;

            double now = Time.realtimeSinceStartupAsDouble;
            bool gotSample = false;
            PhoneReading latest = default;

            while (client.TryDequeue(out var msg))
            {
                if (PhoneProtocol.TryParseControl(msg, out var control))
                {
                    if (control == PhoneControl.PhoneConnected) { needNeutral = true; PhonePresent = true; }
                    else if (control == PhoneControl.PhoneLeft) PhonePresent = false;
                    continue;
                }

                samples.Clear();
                if (PhoneProtocol.ParseSamples(msg, samples) == 0) continue;
                msgCount++;

                foreach (var s in samples)
                {
                    lastSampleRealtime = now;
                    PhonePresent = true;

                    if (needNeutral || (haveRecenter && s.Recenter != lastRecenter))
                    {
                        alpha0 = s.Alpha; beta0 = s.Beta;
                        needNeutral = false;
                        push.Reset();
                    }
                    lastRecenter = s.Recenter; haveRecenter = true;

                    LastAy = s.Ay;
                    RecentPeakAy = Mathf.Max(s.Ay, RecentPeakAy);
                    if (push.Feed(s.Ay, now))
                    {
                        rightFireUntil = Time.time + Settings.PhoneFirePulseSeconds;
                        aimFreezeUntil = Time.time + Settings.PhoneAimFreezeSeconds;
                        PushCount++;
                    }
                    latest = s;
                    gotSample = true;
                }
            }

            if (gotSample)
            {
                LastAlpha = latest.Alpha; LastBeta = latest.Beta; LastGamma = latest.Gamma;
                DeltaAlpha = PhoneAimMapper.WrapDeg(latest.Alpha - alpha0);
                DeltaBeta = latest.Beta - beta0;
            }

            bool fresh = now - lastSampleRealtime <= Settings.PhoneTimeoutSeconds;
            if (!fresh)
            {
                stick = Vector2.zero;
            }
            else if (Time.time < aimFreezeUntil)
            {
                stick = Vector2.zero;
            }
            else
            {
                // iOS alpha grows counter-clockwise (turning left) -> negate so left = negative stick.
                float yaw = -PhoneAimMapper.Map(DeltaAlpha, Settings.PhoneDeadzoneDeg, Settings.PhoneFullSpeedDeg, Settings.PhoneAimMinOut);
                float pitch = PhoneAimMapper.Map(DeltaBeta, Settings.PhoneDeadzoneDeg, Settings.PhoneFullSpeedDeg, Settings.PhoneAimMinOut);
                stick = new Vector2(yaw, pitch);
            }

            // Stats and error logging
            if (Time.unscaledTime - msgWindowStart >= 1f)
            {
                MessagesPerSecond = msgCount / Mathf.Max(0.001f, Time.unscaledTime - msgWindowStart);
                msgCount = 0; msgWindowStart = Time.unscaledTime;
                RecentPeakAy = 0f;
            }
            var err = client.LastError;
            if (err != lastLoggedError)
            {
                lastLoggedError = err;
                if (err != null) Debug.LogWarning("[Phone] relay: " + err);
            }
        }
    }
}
