using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class Tile : MonoBehaviour
{
    public enum EdgeType
    {
        Grass,
        Path
    }

    protected int maxNumberOfVariations;

    [Header("Constraints")]
    public bool mainPath;
    public PointConstraint entryPoint;
    public PointConstraint exitPoint;

    //[HideInInspector]
    public Vector2Int position;

    private int originalMaterialIndex;

    public int GetMaxNumberOfVariations()
    {
        return maxNumberOfVariations;
    }

    /// <summary>Return the rotation for the current <c>variation</c>.</summary>
    protected abstract Quaternion GetRotation(int variation);

    /// <summary>Swap the edges of the <c>child</c> for the current <c>variation</c>.</summary>
    protected abstract void SwapEdges(GameObject child, int variation);

    /// <summary>Generate n <c>numberOfVariations</c> of the current tile: new position, rotation and edges.</summary>
    public void GenerateVariation(int numberOfVariations)
    {
        transform.DestroyImmediateAllChildren();
        List<GameObject> children = new List<GameObject>();
        for (int i = 1; i <= numberOfVariations; i++)
        {
            // Offset position
            Vector3 newPosition = transform.position;
            newPosition.z -= i * (GetComponent<MeshFilter>().sharedMesh.bounds.size.z + 1);
            // Rotate
            Quaternion newRotation = transform.rotation;
            newRotation *= GetRotation(i);
            GameObject child = GameObject.Instantiate(gameObject, newPosition, newRotation);
            // Swap edges
            SwapEdges(child, i);
            children.Add(child);
        }
        foreach (GameObject child in children)
        {
            child.transform.SetParent(transform);
        }
    }

    private int ReplaceMaterial(Material originalMaterial, Material newMaterial)
    {
        Renderer renderer = GetComponent<Renderer>();
        int materialIndex = ArrayUtility.IndexOf<Material>(renderer.sharedMaterials, originalMaterial);
        if (materialIndex != -1)
        {
            ReplaceMaterial(materialIndex, newMaterial);
        }
        return materialIndex;
    }

    private void ReplaceMaterial(int originalMaterialIndex, Material newMaterial)
    {
        if (originalMaterialIndex == -1) return;

        Renderer renderer = GetComponent<Renderer>();
        Material[] materials = renderer.sharedMaterials;
        materials[originalMaterialIndex] = newMaterial;
        renderer.sharedMaterials = materials;
    }

    public void UseDebugMaterial(Material originalMaterialToReplace, Material pathMaterial, Material fixedTileMaterial)
    {
        if (entryPoint.active || exitPoint.active)
        {
            originalMaterialIndex = ReplaceMaterial(originalMaterialToReplace, fixedTileMaterial);
        }
        else if (mainPath)
        {
            originalMaterialIndex = ReplaceMaterial(originalMaterialToReplace, pathMaterial);
        }
    }

    public void UseDebugMaterial(Material originalMaterialToReplace, Material pathMaterial, Material fixedTileMaterial, Vector2Int? position)
    {
        if ((entryPoint.active && entryPoint.position.x == position.Value.x && entryPoint.position.y == position.Value.y) ||
                (exitPoint.active && exitPoint.position.x == position.Value.x && exitPoint.position.y == position.Value.y))
        {
            originalMaterialIndex = ReplaceMaterial(originalMaterialToReplace, fixedTileMaterial);
        }
        else if (mainPath)
        {
            originalMaterialIndex = ReplaceMaterial(originalMaterialToReplace, pathMaterial);
        }
    }

    public void UseDebugMaterial(Material pathMaterial, Material fixedTileMaterial, Vector2Int? position)
    {
        if ((entryPoint.active && entryPoint.position.x == position.Value.x && entryPoint.position.y == position.Value.y) ||
                (exitPoint.active && exitPoint.position.x == position.Value.x && exitPoint.position.y == position.Value.y))
        {
            ReplaceMaterial(originalMaterialIndex, fixedTileMaterial);
        }
        else if (mainPath)
        {
            ReplaceMaterial(originalMaterialIndex, pathMaterial);
        }
    }

    public void UseNormalMaterial(Material originalMaterial)
    {
        ReplaceMaterial(originalMaterialIndex, originalMaterial);
    }



}

[Serializable]
public class PointConstraint
{
    public bool active;
    public Vector2Int position;
}

[CustomPropertyDrawer(typeof(PointConstraint))]
public class PointConstraint_Property : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        position.width *= 0.1f;
        SerializedProperty activeProperty = property.FindPropertyRelative("active");
        EditorGUI.PropertyField(position, activeProperty, GUIContent.none);

        GUI.enabled = activeProperty.boolValue;
        position.x += position.width;
        position.width *= 9f;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("position"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(Tile), true)]
public class Tile_Inspector : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Tile tile = (Tile)target;

        EditorGUILayout.LabelField("ProcGen", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Variations");
        for (int i = 0; i < tile.GetMaxNumberOfVariations(); i++)
        {
            if (GUILayout.Button(i.ToString()))
            {
                tile.GenerateVariation(i);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}