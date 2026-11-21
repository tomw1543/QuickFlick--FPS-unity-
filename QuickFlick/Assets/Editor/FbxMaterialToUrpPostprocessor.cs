using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace QuickFlick.Editor
{
    // Converts FBX materials under Resources/SkyBox/Converted to URP Lit on import.
    public class FbxMaterialToUrpPostprocessor : AssetPostprocessor
    {
        const string Folder = "Resources/SkyBox/Converted";

        void OnPreprocessModel()
        {
            if (!assetPath.Contains(Folder)) return;
            // The MaterialDescription hook only fires in this mode (ImportStandard bypasses it).
            ((ModelImporter)assetImporter).materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        }

        void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] clips)
        {
            if (!assetPath.Contains(Folder)) return;

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null) material.shader = urpLit;

            Vector4 c;
            if (description.TryGetProperty("DiffuseColor", out c) || description.TryGetProperty("BaseColor", out c))
                material.SetColor("_BaseColor", c);

            if (description.TryGetProperty("DiffuseColor", out TexturePropertyDescription map) && map.texture != null)
                material.SetTexture("_BaseMap", map.texture);

            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.3f);

            Debug.Log($"FbxMaterialToUrp: {material.name} shader={material.shader.name} color={material.GetColor("_BaseColor")}");
        }
    }
}
