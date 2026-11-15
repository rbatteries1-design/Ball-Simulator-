using System;
using UnityEditor;
using UnityEngine;

namespace BounceBack.Editor
{
    public static class BounceBackSkyboxes
    {
        private const string TextureFolder = "Assets/Skyboxes";
        private const string MaterialFolder = "Assets/Materials/Skyboxes";

        [MenuItem("Bounce Back/Create Skybox Materials")]
        public static void CreateSkyboxMaterials()
        {
            EnsureMaterialFolder();

            Shader shader = Shader.Find("Skybox/Panoramic");
            if (shader == null)
            {
                throw new InvalidOperationException("Unity's Skybox/Panoramic shader could not be found.");
            }

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureFolder });
            if (textureGuids.Length == 0)
            {
                throw new InvalidOperationException("No skybox textures were found in " + TextureFolder + ".");
            }

            foreach (string guid in textureGuids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    continue;
                }

                string materialPath = MaterialFolder + "/" + texture.name + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader);
                    material.name = texture.name;
                    AssetDatabase.CreateAsset(material, materialPath);
                }

                material.shader = shader;
                material.SetTexture("_MainTex", texture);
                material.SetColor("_Tint", Color.white);
                material.SetFloat("_Exposure", 1f);
                material.SetFloat("_Rotation", 0f);
                EditorUtility.SetDirty(material);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Skybox materials created in " + MaterialFolder + ".");
        }

        private static void EnsureMaterialFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Skyboxes");
            }
        }
    }
}
