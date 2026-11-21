# Net

Namespace: `QuickFlick.Net`

Phone-controller networking: outbound websocket to the Cloudflare relay (contract: `PHONE-CONTROLLER-PLAN.md` section 4).

**Rule:** background threads (`RelayClient`) touch only the `ConcurrentQueue` and volatile fields. No `UnityEngine` API off the main thread; the main thread logs `LastError`.

**Rule:** `RelayClient` and `RelayUrls` have no `UnityEngine` dependency. Always `Stop()` on quit/disable/`beforeAssemblyReload`.

**Rule:** `.env` is read only for `PHONE_RELAY_URL` and `PHONE_ROOM`. Never read other keys into state, never log `.env` values.
