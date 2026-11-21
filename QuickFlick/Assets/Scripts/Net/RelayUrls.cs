using System.Text.RegularExpressions;

namespace QuickFlick.Net
{
    // Pure helpers, no UnityEngine.
    public static class RelayUrls
    {
        static readonly Regex RoomRx = new Regex("^[a-z0-9-]{3,40}$");

        public static bool IsValidRoom(string room) => room != null && RoomRx.IsMatch(room);

        // https://host -> wss://host/ws?room=<room>&role=game (http -> ws for the local dev relay)
        public static string GameSocketUrl(string relayUrl, string room)
        {
            var b = Base(relayUrl, room);
            if (b == null) return null;
            string wsBase;
            if (b.StartsWith("https://")) wsBase = "wss://" + b.Substring(8);
            else if (b.StartsWith("http://")) wsBase = "ws://" + b.Substring(7);
            else return null;
            return wsBase + "/ws?room=" + room + "&role=game";
        }

        public static string JoinUrl(string relayUrl, string room)
        {
            var b = Base(relayUrl, room);
            return b == null ? null : b + "/?room=" + room;
        }

        static string Base(string relayUrl, string room)
        {
            if (string.IsNullOrWhiteSpace(relayUrl) || !IsValidRoom(room)) return null;
            var b = relayUrl.Trim().TrimEnd('/');
            if (b.Length == 0) return null;
            return b;
        }
    }
}
