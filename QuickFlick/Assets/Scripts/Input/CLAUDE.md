# Input

Namespace: `QuickFlick.Input`

Device abstraction for all player input. Gameplay code never talks to `UnityEngine.Input`, Input System actions, or `SerialPort` directly — only to `IControllerInput`.

**Interface:** `IControllerInput { Vector2 LeftStick; Vector2 RightStick; bool LeftFire; bool RightFire; }`

Tonight: `MouseInput` writes virtual-tilt offsets into both sticks (so `StickCombiner` sees them moving together → 100% speed). Tomorrow: `SerialInput` reads raw stick values on a background thread.

**Rule:** `StickCombiner` is pure (no `UnityEngine.*`). It stays testable in EditMode.

**Rule:** anything reading serial runs on a background thread and never calls Unity APIs. Close the `SerialPort` in `OnDisable`/`OnApplicationQuit`.

**Rule:** parse floats with `CultureInfo.InvariantCulture`.
