using System.IO;
using UnityEngine;

namespace QuickFlick.Net
{
    // Main thread only. Reads ONLY PHONE_RELAY_URL and PHONE_ROOM from the repo-root .env.
    // Every other key/value is skipped unread-into-state; nothing is logged here.
    public static class EnvConfig
    {
        public static bool TryLoad(out string relayUrl, out string room, out string error)
        {
            relayUrl = null; room = null; error = null;
            string[] lines;
            try
            {
                var path = Path.Combine(Application.dataPath, "..", "..", ".env");
                if (!File.Exists(path)) { error = ".env not found"; return false; }
                lines = File.ReadAllLines(path);
            }
            catch (System.Exception e) { error = ".env unreadable: " + e.GetType().Name; return false; }

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                var key = line.Substring(0, eq).Trim();
                if (key != "PHONE_RELAY_URL" && key != "PHONE_ROOM") continue;
                var val = Unquote(line.Substring(eq + 1).Trim());
                if (key == "PHONE_RELAY_URL") relayUrl = val; else room = val;
            }

            if (string.IsNullOrEmpty(relayUrl)) { error = "PHONE_RELAY_URL missing in .env"; return false; }
            if (string.IsNullOrEmpty(room)) { error = "PHONE_ROOM missing in .env"; return false; }
            if (!RelayUrls.IsValidRoom(room)) { error = "PHONE_ROOM invalid (need a-z, 0-9, - ; 3-40 chars)"; return false; }
            if (RelayUrls.GameSocketUrl(relayUrl, room) == null) { error = "PHONE_RELAY_URL invalid (need http:// or https://)"; return false; }
            return true;
        }

        static string Unquote(string v)
        {
            if (v.Length >= 2 && ((v[0] == '"' && v[v.Length - 1] == '"') || (v[0] == '\'' && v[v.Length - 1] == '\'')))
                return v.Substring(1, v.Length - 2);
            return v;
        }
    }
}
