# Bootstrap

Namespace: `QuickFlick.Bootstrap`

Owns the single entry point (`GameBootstrap`) attached to the Bootstrap GameObject in `Main.unity`. `GameBootstrap` instantiates everything the scene does not hold statically: controllers, combiner, spawner, HUD, post-fx, audio.

**Rule:** `GameBootstrap` is the ONLY place that wires systems together. If two systems need each other, they meet here, not via `FindObjectOfType` or singletons.

**Rule:** no gameplay logic lives here. Composition only.

**Rule:** scene-agnostic. `GameBootstrap` must work in any scene that has the 5 bootstrap objects. No hard-coded references to Main.unity content — Space Station / Mars / Asteroid Field scenes come later.
