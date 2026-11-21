using QuickFlick.Camera;
using QuickFlick.Combat;
using QuickFlick.Input;
using UnityEngine;

namespace QuickFlick.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        static GameBootstrap instance;

        static readonly System.Collections.Generic.HashSet<string> SceneWhitelist = new()
        {
            "Main Camera", "Directional Light", "Global Volume", "EventSystem", "Bootstrap",
        };

        IControllerInput input;
        TurretCamera turret;
        MegaShotDetector mega;
        Cannon left, right;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SelfRegister()
        {
            if (FindFirstObjectByType<GameBootstrap>() != null) return;
            DontDestroyOnLoad(new GameObject("Bootstrap").AddComponent<GameBootstrap>());
        }

        void Awake()
        {
            // Scene objects are not loaded yet in SelfRegister, so a scene-placed Bootstrap shows up here instead.
            if (instance != null) { Destroy(this); return; }
            instance = this;
        }

        void Start()
        {
            if (instance != this) return;

            Debug.Log($"[Boot] fog disabled; was mode={RenderSettings.fogMode} dens={RenderSettings.fogDensity} color={RenderSettings.fogColor}");
            RenderSettings.fog = false;

            // Runs first: later code spawns scene-root objects (Spawner, EnemyPool) that must survive.
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (!SceneWhitelist.Contains(go.name.Trim())) Destroy(go);

            var cam = UnityEngine.Camera.main.gameObject;
            BuildShipPlatform(cam.transform.position);
            BuildSun();
            BuildPlanet();

            // SerialInput opens its port synchronously in OnEnable, so IsOpen is valid right after AddComponent.
            var serial = gameObject.AddComponent<SerialInput>();
            if (serial.IsOpen)
            {
                input = serial;
                Debug.Log($"[Bootstrap] Input: Serial ({serial.PortName})");
            }
            else
            {
                Destroy(serial);
                input = gameObject.AddComponent<MouseInput>();
                Debug.Log("[Bootstrap] Input: Mouse (serial unavailable)");
            }

            turret = cam.GetComponent<TurretCamera>();
            if (turret == null) turret = cam.AddComponent<TurretCamera>();

            QuickFlick.UI.StarfieldBuilder.Install();

            gameObject.AddComponent<ProjectilePool>();
            mega = gameObject.AddComponent<MegaShotDetector>();
            left = MakeCannon("CannonL", cam.transform, new Vector3(-0.3f, -0.2f, 0.5f));
            right = MakeCannon("CannonR", cam.transform, new Vector3(0.3f, -0.2f, 0.5f));

            new GameObject("EnemyPool").AddComponent<QuickFlick.Enemies.EnemyPool>();
            new GameObject("Spawner").AddComponent<QuickFlick.Systems.Spawner>();

            foreach (var t in new[] { typeof(QuickFlick.Systems.GameState), typeof(QuickFlick.Systems.PlayerHp), typeof(QuickFlick.Systems.Score), typeof(QuickFlick.Systems.WaveManager) })
            {
                var go = new GameObject(t.Name);
                go.transform.SetParent(transform, false);
                go.AddComponent(t);
            }

            Debug.Log("Boot OK");
        }

        // World-space (not parented to the camera) so it stays level while the camera pitches.
        static void BuildShipPlatform(Vector3 camPos)
        {
            var root = new GameObject("ShipPlatform");
            root.transform.position = camPos;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.87f, 0.87f, 0.87f)); // #DEDEDE light grey
            mat.SetFloat("_Metallic", 0f);   // matte: no skybox reflections
            mat.SetFloat("_Smoothness", 0f);

            MakeDisc(root.transform, mat, "Body", new Vector3(7f, 0.3f, 7f), -1.6f);
            MakeDisc(root.transform, mat, "Rim", new Vector3(8.5f, 0.1f, 8.5f), -1.9f);

            // Children are parented first, then the root is marked.
            DontDestroyOnLoad(root);
        }

        void BuildSun()
        {
            var sunPos = new Vector3(300f, 150f, 300f);

            var sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sun.name = "Sun";
            Destroy(sun.GetComponent<Collider>());
            sun.transform.SetParent(transform, false);
            sun.transform.position = sunPos;
            sun.transform.localScale = Vector3.one * 60f;

            var warm = new Color32(0xFF, 0xF8, 0xE0, 255);
            // URP Lit, not Unlit/Texture: Unlit ignores emission, and HDR >1 is what makes bloom flare.
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", Color.white);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white * 8f); // HDR intensity: one number to tweak the glare
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            sun.GetComponent<Renderer>().sharedMaterial = mat;

            Light light = null;
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.name == "Directional Light") { light = go.GetComponent<Light>(); break; }
            if (light == null)
            {
                light = new GameObject("Directional Light").AddComponent<Light>();
                light.type = LightType.Directional;
            }
            light.transform.rotation = Quaternion.LookRotation(-Vector3.Normalize(sunPos));
            light.color = warm;
            light.intensity = 1.3f;
        }

        void BuildPlanet()
        {
            var prefab = Resources.Load<GameObject>("SkyBox/Converted/Planet by Quaternius - 9g1aIbfR9Y");
            if (prefab == null) { Debug.LogWarning("BuildPlanet: FBX prefab not found in Resources/SkyBox/Converted/"); return; }
            var planet = Instantiate(prefab);
            planet.name = "Planet";
            planet.transform.SetParent(transform, false);
            planet.transform.position = new Vector3(-250f, 80f, 320f);
            planet.transform.localScale = Vector3.one * 40f;
            foreach (var c in planet.GetComponentsInChildren<Collider>()) Destroy(c);
            FixImportedMaterials(planet, fallback: new Color(0.80f, 0.37f, 0.07f), emissionStrength: 0.1f);
            Debug.Log($"Planet: loaded FBX prefab, children={planet.transform.childCount}");
        }

        // FBX-imported materials often ship with Standard shader or stale keywords; URP then renders them white.
        // Clone each material, force URP Lit, copy the colour from whichever property the old shader used.
        internal static void FixImportedMaterials(GameObject root, Color fallback, float emissionStrength = 0.4f)
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
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", c * emissionStrength); // self-lit so dark enemies read against the starfield
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    mats[i] = m;
                    Debug.Log($"FixImportedMaterials: {r.name} slot {i} src-shader={src.shader.name} color={c}");
                }
                r.sharedMaterials = mats;
            }
        }

        static Shader UnlitShader() =>
            Shader.Find("Unlit/Texture")
            ?? Shader.Find("Legacy Shaders/Unlit/Texture")
            ?? Shader.Find("Universal Render Pipeline/Unlit");

        // A bound texture renders regardless of shader keyword state, unlike a bare _BaseColor.
        static Texture2D SolidTexture(Color c)
        {
            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            t.SetPixels(new[] { c, c, c, c });
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            t.Apply();
            return t;
        }

        // Radial blend: core at the UV centre, edge at the corners.
        static Texture2D PlanetTexture(Color core, Color edge)
        {
            const int n = 256;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = new Vector2(x - n / 2f, y - n / 2f).magnitude / (n * 0.7071f);
                    px[y * n + x] = Color.Lerp(core, edge, Mathf.Clamp01(d));
                }
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            t.Apply();
            return t;
        }

        static void MakeDisc(Transform parent, Material mat, string name, Vector3 scale, float y)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            d.name = name;
            Destroy(d.GetComponent<Collider>());
            d.GetComponent<Renderer>().sharedMaterial = mat;
            d.transform.SetParent(parent, false);
            d.transform.localPosition = new Vector3(0f, y, 0f);
            d.transform.localScale = scale;
        }

        static Cannon MakeCannon(string name, Transform parent, Vector3 offset)
        {
            var c = new GameObject(name).AddComponent<Cannon>();
            c.transform.SetParent(parent, false);
            c.muzzle = c.transform;
            c.muzzleOffset = offset;
            return c;
        }

        void Update()
        {
            if (turret == null) return;
            turret.Apply(StickCombiner.Combine(input.LeftStick, input.RightStick));

            mega.Tick(input.LeftFire, input.RightFire);
            if (left.TryFire(input.LeftFire, mega.LeftMegaPending)) mega.ConsumeLeft();
            if (right.TryFire(input.RightFire, mega.RightMegaPending)) mega.ConsumeRight();
        }
    }
}
