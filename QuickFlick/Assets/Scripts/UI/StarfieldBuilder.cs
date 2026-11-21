using UnityEngine;

namespace QuickFlick.UI
{
    public static class StarfieldBuilder
    {
        public static void Install(int starCount = 800, int faceSize = 512, int seed = 1337)
        {
            var cubemap = new Cubemap(faceSize, TextureFormat.RGBA32, false);
            int perFace = starCount / 6;

            for (int face = 0; face < 6; face++)
            {
                var pixels = new Color[faceSize * faceSize];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black;

                var rng = new System.Random(seed + face);
                for (int s = 0; s < perFace; s++)
                {
                    float b = 0.6f + 0.4f * (float)rng.NextDouble();
                    pixels[rng.Next(pixels.Length)] = new Color(b, b, b, 1f);
                }

                cubemap.SetPixels(pixels, (CubemapFace)face);
            }
            cubemap.Apply();

            var material = new Material(Shader.Find("Skybox/Cubemap"));
            material.SetTexture("_Tex", cubemap);
            RenderSettings.skybox = material;
            DynamicGI.UpdateEnvironment();
        }
    }
}
