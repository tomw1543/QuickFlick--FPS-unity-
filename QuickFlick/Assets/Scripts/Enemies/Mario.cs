using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Mario : LevelApproachEnemy
    {
        // Measured in Blender: glb bbox 51.9 (arms) x 66.5 (height) x 28.0 (depth), height along glTF Z
        // (model imports lying down), so ModelScale = 2 / 66.46 gives ~2 units tall.
        protected override int MaxHp => 1;
        protected override int Damage => 1;
        protected override float MinSpeed => 3.0f;
        protected override float MaxSpeed => 8.0f;
        protected override float ColliderRadius => 1.0f;
        protected override string ModelPath => "SkyBox/Converted/Budget Mario by Teh LaughingMan - cQMXnePNv-0";
        protected override float ModelScale => 0.0301f;
        // Tunable: tilt stands the lying model up (try 90 if he lies the wrong way); yaw 0 or 180 if he walks backwards.
        protected override float ModelTiltX => -90f;
        protected override float ModelYawOffset => 0f;
        protected override Color Colour => new Color(0.90f, 0.15f, 0.15f); // red (fallback if Palette is nulled)
        // Mario palette: red cap/shirt, blue overalls, peach skin, yellow buttons, brown shoes, white gloves.
        // Cycles per material slot — slot order is FBX-dependent, re-order if hat ends up blue.
        static readonly Color[] MarioPalette = {
            new Color(0.90f, 0.15f, 0.15f), // red
            new Color(0.15f, 0.30f, 0.80f), // blue
            new Color(0.98f, 0.80f, 0.65f), // peach skin
            new Color(0.95f, 0.85f, 0.20f), // yellow
            new Color(0.40f, 0.22f, 0.10f), // brown
            new Color(0.95f, 0.95f, 0.95f), // white gloves
        };
        protected override Color[] Palette => MarioPalette;

        AudioSource src;
        void OnEnable()
        {
            if (src == null)
            {
                src = gameObject.AddComponent<AudioSource>();
                src.spatialBlend = 0f;
                src.loop = true;
                src.clip = Resources.Load<AudioClip>("Audio/Sfx/freesound_community-mariomp3-14599");
            }
            if (src.clip != null) src.Play();
        }
        void OnDisable() { if (src != null) src.Stop(); }
    }
}
