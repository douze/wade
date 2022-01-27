using UnityEngine;

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
  
}