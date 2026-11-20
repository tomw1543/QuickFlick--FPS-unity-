# CONTEXT.md — QuickFlick Glossary

Terms of art for this project. Keep this file free of implementation detail — it defines *what things are*, not *how they work*. Update it the moment a new term is agreed.

## Terms

**Stick.** One analog 2-axis input. A controller has two Sticks: left and right.

**Combiner.** `StickCombiner`. Merges left + right Stick into `(lookYaw, lookPitch, speedScale)`. Parallel Sticks yield full speed; opposing Sticks yield zero.

**Cannon.** One of two forward-facing weapons on the player turret. Fires a Projectile. The player has a Left Cannon and a Right Cannon, bound to the left and right Fire inputs.

**MegaShot.** A single shot of 3× damage emitted when both Cannons fire within a 150 ms window. (Supersedes the earlier term "Double Shot".)

**Wave.** A timed spawn group of Enemies. A Wave is Cleared when all its Enemies are dead; clearing a Wave starts the next.

**Drone.** The baseline Enemy. Angular red shard, 1 HP, flies in a straight line toward the player.

**Weaver.** Future Enemy (not tonight). 2 HP, sine-weave approach.

**Brute.** Future Enemy (not tonight). 4 HP, slow, returns fire.

**Virtual tilt.** The mouse's offset from screen centre, interpreted as a Stick deflection. Used only by `MouseInput` as a stand-in for real analog Sticks. Replaced by raw stick values when the Arduino lands.

**Controller.** The input device abstracted behind `IControllerInput`. Mouse tonight; Arduino tomorrow. Code never talks to a specific Controller — only to the interface.
