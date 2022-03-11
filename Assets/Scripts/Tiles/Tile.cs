using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>A <c>Tile</c> is... a tile.</summary>
[ExecuteInEditMode]
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
    public float frequency = 1.0f;

    [Header("Props")]
    public bool trees = false;
    public int maxPropsWeight = 5;

    [HideInInspector]
    public Vector2Int position;

    private int originalMaterialIndex;
    private Renderer tileRenderer;

    public int GetMaxNumberOfVariations()
    {
        return maxNumberOfVariations;
    }

    public void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
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

    /// <summary>Use the <c>pathMaterial</c> or <c>fixedTileMaterial</c> (depending on the <c>position</c>) instead of the <c>originalMaterialToReplace</c>.</summary>
    public void UseDebugMaterial(Material originalMaterialToReplace, Material pathMaterial, Material fixedTileMaterial, Vector2Int position)
    {
        bool isEntryPoint = entryPoint.active && entryPoint.position.x == position.x && entryPoint.position.y == position.y;
        bool isExitPoint = exitPoint.active && exitPoint.position.x == position.x && exitPoint.position.y == position.y;
        if (isEntryPoint || isExitPoint)
        {
            originalMaterialIndex = tileRenderer.ReplaceSharedMaterial(originalMaterialToReplace, fixedTileMaterial);
        }
        else if (mainPath)
        {
            originalMaterialIndex = tileRenderer.ReplaceSharedMaterial(originalMaterialToReplace, pathMaterial);
        }
    }

    /// <summary>Revert to the <c>originalMaterial</c>.</summary>
    public void UseNormalMaterial(Material originalMaterial)
    {
        tileRenderer.UseSharedMaterialOnIndex(originalMaterialIndex, originalMaterial);
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