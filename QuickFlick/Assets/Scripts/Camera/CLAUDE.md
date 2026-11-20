# Camera

Namespace: `QuickFlick.Camera`

Turret-style look rig. The camera does not translate — only rotates from `StickCombiner` output.

`TurretCamera` reads `(lookYaw, lookPitch)` each frame and applies them to the Main Camera transform. Pitch clamped ±70°. Yaw unlimited.

**Rule:** no look input is sampled here. Camera is downstream of `IControllerInput` + `StickCombiner`.
