using MSCLoader;
using System.Collections.Generic;
using UnityEngine;

namespace GhostCar
{
    public class AlphaManager : MonoBehaviour
    {
        private class GhostMaterial
        {
            public Material mat;
            public Material originalMat;
            public float originalAlpha;
            public float originalGloss;
            public float originalMetallic;
        }

        private List<GhostMaterial> ghostMaterials = new List<GhostMaterial>();

        public void CollectMaterials(GameObject ghost, float initialUserAlpha = 0.4f)
        {
            ghostMaterials.Clear();

            foreach (var renderer in ghost.GetComponentsInChildren<Renderer>(true))
            {
                Material[] clonedMaterials = new Material[renderer.materials.Length];

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    var originalMat = renderer.materials[i];
                    if (originalMat == null) continue;

                    var clonedMat = new Material(originalMat);

                    float gloss = originalMat.HasProperty("_Glossiness") ? originalMat.GetFloat("_Glossiness") : 0f;
                    float metallic = originalMat.HasProperty("_Metallic") ? originalMat.GetFloat("_Metallic") : 0f;

                    SetupMaterialTransparent(clonedMat);

                    float originalAlpha = clonedMat.color.a;
                    SetMaterialAlpha(clonedMat, originalAlpha, initialUserAlpha);

                    ghostMaterials.Add(new GhostMaterial
                    {
                        mat = clonedMat,
                        originalMat = originalMat,
                        originalAlpha = originalAlpha,
                        originalGloss = gloss,
                        originalMetallic = metallic
                    });

                    clonedMaterials[i] = clonedMat;
                }

                renderer.materials = clonedMaterials;
            }
        }

        public void SetAlpha(float userAlpha)
        {
            ModConsole.Print($"SetAlpha: {userAlpha}");

            foreach (var gm in ghostMaterials)
            {
                float finalAlpha = Mathf.Clamp01(gm.originalAlpha * userAlpha);

                if (finalAlpha >= 0.99f)
                    SetupMaterialOpaque(gm.mat, gm.originalAlpha, gm.originalGloss, gm.originalMetallic);
                else
                {
                    SetupMaterialTransparent(gm.mat);
                    SetMaterialAlpha(gm.mat, gm.originalAlpha, userAlpha);
                }
            }
        }

        private void SetMaterialAlpha(Material mat, float originalAlpha, float userAlpha)
        {
            Color c = mat.color;
            c.a = Mathf.Clamp01(originalAlpha * userAlpha);
            mat.color = c;
        }

        private void SetupMaterialTransparent(Material mat)
        {
            mat.shader = Shader.Find("Standard");
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", 0f);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0f);
        }

        private void SetupMaterialOpaque(Material mat, float originalAlpha, float originalGloss, float originalMetallic)
        {
            mat.shader = Shader.Find("Standard");
            mat.SetFloat("_Mode", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;

            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", originalGloss);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", originalMetallic);

            Color c = mat.color;
            c.a = originalAlpha;
            mat.color = c;
        }
    }
}
