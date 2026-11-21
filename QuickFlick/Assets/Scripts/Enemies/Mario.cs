using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Mario : LevelApproachEnemy
    {
        // Measured in Blender: glb bbox 51.9 (arms) x 66.5 (height) x 28.0 (depth), height along glTF Z
        // (model imports lying down), so ModelScale = 2 / 66.46 gives ~2 units tall.
        protected override int MaxHp => 1;
        protected override int Damage => 1;
        protected override float MinSpeed => 1.5f;
        protected override float MaxSpeed => 4.0f;
        protected override float ColliderRadius => 1.0f;
        protected override string ModelPath => "SkyBox/Converted/Budget Mario by Teh LaughingMan - cQMXnePNv-0";
        protected override float ModelScale => 0.0301f;
        // Tunable: tilt stands the lying model up (try 90 if he lies the wrong way); yaw 0 or 180 if he walks backwards.
        protected override float ModelTiltX => -90f;
        protected override float ModelYawOffset => 0f;
        protected override Color Colour => Color.white;
    }
}
