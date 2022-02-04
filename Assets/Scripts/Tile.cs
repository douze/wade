using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

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