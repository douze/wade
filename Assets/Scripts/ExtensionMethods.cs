using UnityEngine;
using UnityEditor;

public static class ExtensionMethods
{

    /// <summary>Destroy all the GameObject child instances from the <c>transform</c>.</summary>
    public static void DestroyImmediateAllChildren(this Transform transform)
    {
        while (transform.childCount > 0)
        {
            GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    /// <summary>Replace the <c>originalMaterial</c> with the <c>newMaterial</c>.</summary>
    public static int ReplaceSharedMaterial(this Renderer renderer, Material originalMaterial, Material newMaterial)
    {
        int materialIndex = ArrayUtility.IndexOf<Material>(renderer.sharedMaterials, originalMaterial);
        if (materialIndex != -1)
        {
            renderer.UseSharedMaterialOnIndex(materialIndex, newMaterial);
        }
        return materialIndex;
    }

    /// <summary>Use the <c>newMaterial</c> on the shared materials at <c>originalMaterialIndex</c>.</summary>
    public static void UseSharedMaterialOnIndex(this Renderer renderer, int originalMaterialIndex, Material newMaterial)
    {
        if (originalMaterialIndex == -1) return;

        Material[] materials = renderer.sharedMaterials;
        materials[originalMaterialIndex] = newMaterial;
        renderer.sharedMaterials = materials;
    }
  
}