using MSCLoader;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Effects;

namespace GhostCar
{
    public class AlphaManager : MonoBehaviour
    {
        private class GhostMaterial
        {
            public Material mat;
            public Material origMat;
            public float originalAlpha;
        }

        private List<GhostMaterial> ghostMaterials = new List<GhostMaterial>();
        private readonly Material clone;

        public void CollectMaterials(GameObject ghost, float initialUserAlpha = 0.4f)
        {
            ghostMaterials.Clear();

            foreach (var renderer in ghost.GetComponentsInChildren<Renderer>(true))
            {
                Material[] clonedMaterials = new Material[renderer.materials.Length];

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material originalMat = renderer.materials[i];
                    if (originalMat == null) continue;

                    Material clonedMat = new Material(originalMat); // clone it
                    SetupMaterial(clonedMat); // apply blend mode, shader settings

                    float originalAlpha = clonedMat.color.a;
                    SetMaterialAlpha(clonedMat, originalMat, originalAlpha, initialUserAlpha);

                    ghostMaterials.Add(new GhostMaterial
                    {
                        mat = clonedMat,
                        originalAlpha = originalAlpha
                    });

                    clonedMaterials[i] = clonedMat;
                }

                renderer.materials = clonedMaterials; // assign cloned materials back to renderer
            }
        }

        public void SetAlpha(float userAlpha)
        {
            ModConsole.Print($"SetAlpha: {userAlpha}");

            foreach (var gm in ghostMaterials)
            {
                SetMaterialAlpha(gm.mat, gm.origMat, gm.originalAlpha, userAlpha);
                //SetMaterialAlpha(gm.mat, 1, userAlpha);
            }
        }

        private void SetupMaterial(Material mat)
        {
            /* mat.shader = Shader.Find("Standard");

            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000; */

            mat.shader = Shader.Find("Transparent/Diffuse");
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1);
        }

        public static void SetMaterialAlpha(Material mat, Material origMat, float originalAlpha, float userAlpha)
        {
            if (mat == null) return;

            Color c = mat.color;
            c.a = originalAlpha * userAlpha;
            mat.color = c;
        }

        private void SetMaterialAlphaX(Material mat, Material origMat, float originalAlpha, float userAlpha)
        {
            float finalAlpha = Mathf.Clamp01(originalAlpha * userAlpha);

            ModConsole.Print($"SetMaterialAlpha: {mat}, {originalAlpha}, {userAlpha}, {finalAlpha}");

            //Color c = mat.color;
            //c.a = finalAlpha;
            //mat.color = c;

            if (finalAlpha >= 0.99f)
            {
                // Set to opaque
                mat.SetFloat("_Mode", 0); // 0 = Opaque

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                mat.renderQueue = -1; // Default render queue

                // Also restore original alpha value
                Color c = mat.color;
                c.a = originalAlpha; // whatever you cached in CollectMaterials
                mat.color = c;
            }
            else
            {
                // Set to transparent
                mat.SetFloat("_Mode", 3); // 3 = Transparent in Unity Standard shader
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                // Optional: apply alpha by multiplying original by user value
                Color c = mat.color;
                c.a = finalAlpha;
                mat.color = c;

                //SetAlpha(userAlpha);
                //mat.shader = Shader.Find("Transparent/Diffuse");
                //mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, finalAlpha);
            }
        }
    }
}
