# FBX-IMPORT.md — loading Poly/Quaternius models in QuickFlick

Reference for anything that drops a `.glb`/`.fbx` model into the scene (planet, asteroids, crabs, octopus, drones). Use this pattern verbatim unless the model ships a working embedded texture you actually want.

## The problem

FBXs exported by Blender or shipped by Quaternius/Poly arrive with one of:

- Standard shader materials (URP renders them white/pink).
- URP Lit materials with no colour set — default `_BaseColor` = grey (0.8, 0.8, 0.8).
- URP Lit materials with `_Color` set but not `_BaseColor`, or vice-versa.
- Stale shader keywords from the source pipeline.

Any of these causes the "shader is right in the log, pixel is still white" ghost bug. Scene fog made it worse in Phase 7 — that's fixed now (`RenderSettings.fog = false` in `GameBootstrap.Start`).

## The method — `FixImportedMaterials`

Defined in `QuickFlick/Assets/Scripts/Bootstrap/GameBootstrap.cs`. Static, callable from anywhere. Clones every material on the imported hierarchy to a fresh URP Lit with a known colour.

```csharp
static void FixImportedMaterials(GameObject root, Color fallback)
{
    var urpLit = Shader.Find("Universal Render Pipeline/Lit");
    if (urpLit == null) { Debug.LogWarning("FixImportedMaterials: URP Lit shader not found"); return; }
    foreach (var r in root.GetComponentsInChildren<Renderer>())
    {
        var mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            var src = mats[i];
            if (src == null) { mats[i] = new Material(urpLit) { color = fallback }; continue; }
            Color c = src.HasProperty("_BaseColor") ? src.GetColor("_BaseColor")
                    : src.HasProperty("_Color")    ? src.GetColor("_Color")
                    : fallback;
            bool nearBlack = c.r + c.g + c.b < 0.03f;
            bool urpDefaultGrey = Mathf.Abs(c.r - 0.8f) < 0.01f && Mathf.Abs(c.g - 0.8f) < 0.01f && Mathf.Abs(c.b - 0.8f) < 0.01f;
            if (nearBlack || urpDefaultGrey) c = fallback; // no real colour was ever assigned
            var m = new Material(urpLit);
            m.shaderKeywords = new string[0];
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_Smoothness", 0.3f);
            mats[i] = m;
        }
        r.sharedMaterials = mats;
    }
}
```

## Caller template

```csharp
void SpawnAsteroid(Vector3 position, float scale)
{
    var prefab = Resources.Load<GameObject>("SkyBox/Converted/Asteroid by Poly by Google - enaIlQWET9a");
    if (prefab == null) { Debug.LogWarning("SpawnAsteroid: FBX not found"); return; }
    var go = Instantiate(prefab);
    go.name = "Asteroid";
    go.transform.position = position;
    go.transform.localScale = Vector3.one * scale;
    foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c); // or keep, if hit detection uses them
    FixImportedMaterials(go, fallback: new Color(0.45f, 0.42f, 0.38f)); // grey-brown rock
}
```

## Rules

1. **Always call `FixImportedMaterials` right after `Instantiate`.** Before setting position, scale, children — doesn't matter, as long as it runs before the first frame renders the object.
2. **Pass a thematic fallback colour.** Even if the FBX has a valid colour, the fallback catches cases where the material shipped default grey (0.8, 0.8, 0.8) or near-black. Pick per model type:
   - Planet: `(0.80, 0.37, 0.07)` — rust orange
   - Asteroid: `(0.45, 0.42, 0.38)` — grey-brown rock
   - Crab: tune later
   - Octopus: purple-green mix — pick on sight
   - Drone/Satellite: `(0.55, 0.60, 0.65)` — metallic grey
3. **Load via `Resources.Load<GameObject>("SkyBox/Converted/<exact name>")`.** The FBX must live under `QuickFlick/Assets/Resources/SkyBox/Converted/` (where `scripts/glb_to_fbx.py` outputs). Use the file name WITHOUT the `.fbx` extension in the load string.
4. **Destroy child colliders** unless you need them for hits. FBX models ship with mesh colliders that hurt perf and interact oddly with triggers.
5. **Never hand-edit the imported FBX materials in the Unity inspector.** `FixImportedMaterials` runs at runtime and clones materials anyway — editor edits get discarded. If you need a persistent custom look, bake it into the `fallback` colour or extend this method.
6. **One-shot, never pooled.** `Instantiate` + `FixImportedMaterials` runs per spawn. If pooling enemies later, call `FixImportedMaterials` once on the pooled prefab, not per spawn.

## Known gotchas

- **URP Lit shader strip on standalone builds.** `Shader.Find("Universal Render Pipeline/Lit")` can return null in a build if the shader is stripped. Not a problem in editor Play. If it bites: add URP Lit to the project's Always Included Shaders list.
- **Multiple renderers in one FBX.** The method iterates `GetComponentsInChildren<Renderer>` — all slots on all children get the same fallback. If different sub-meshes need different colours, extend the method with per-renderer logic rather than branching inside the caller.
- **Model loaded white despite this method:** check `RenderSettings.fog` first. Scene fog overrode fragments to near-white in Phase 7 — the `GameBootstrap.Start` disable line must still be in place.

## File map

- Method: `QuickFlick/Assets/Scripts/Bootstrap/GameBootstrap.cs` (`FixImportedMaterials`)
- Converted FBXs: `QuickFlick/Assets/Resources/SkyBox/Converted/*.fbx`
- Source `.glb`s: `QuickFlick/Assets/Resources/SkyBox/*.glb`
- Blender conversion script: `scripts/glb_to_fbx.py`
