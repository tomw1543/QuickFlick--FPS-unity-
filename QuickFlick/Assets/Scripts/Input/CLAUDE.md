# Input

Namespace: `QuickFlick.Input`

Device abstraction for all player input. Gameplay code never talks to `UnityEngine.Input`, Input System actions, or `SerialPort` directly — only to `IControllerInput`.

**Interface:** `IControllerInput { Vector2 LeftStick; Vector2 RightStick; bool LeftFire; bool RightFire; }`

`SerialInput` is the default when the Arduino is connected (MMA8452Q via the Uno sketch at `Firmware/Recon/Recon.ino`); it writes the tilt vector into both sticks and fires on a g-spike. `MouseInput` is the fallback and does the same with virtual tilt (so `StickCombiner` sees both sticks moving together → 100% speed).

**Rule:** `StickCombiner` is pure (no `UnityEngine.*`). It stays testable in EditMode.

**Rule:** anything reading serial runs on a background thread and never calls Unity APIs. Close the `SerialPort` in `OnDisable`/`OnApplicationQuit`.

**Rule:** parse floats with `CultureInfo.InvariantCulture`.

`PhoneInput` (phone over the Cloudflare relay, see `PHONE-CONTROLLER-PLAN.md`) drains the relay queue every frame, even when not selected; velocity aim from orientation, right-cannon pulse on a push. `InputSelector` owns Mouse/Serial/Phone and is what `GameBootstrap` uses: keys 7 = Mouse, 8 = Arduino, 9 = Phone. Startup default: Arduino if its port opens, else Mouse. Inactive Mouse/Serial are disabled (unlock cursor / close port). **`PhoneInput` is never disabled** so it keeps draining.
