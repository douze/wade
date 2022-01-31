using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public abstract class Tile : MonoBehaviour
{
    public enum EdgeType
    {
        Grass,
        Path
    }

    protected int maxNumberOfVariations;

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

[CustomEditor(typeof(Tile), true)]
public class Tile_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Tile tile = (Tile)target;

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Variations");
        for (int i = 0; i < tile.GetMaxNumberOfVariations(); i++)
        {
            if (GUILayout.Button(i.ToString()))
            {
                tile.GenerateVariation(i);
            }
        }
        GUILayout.EndHorizontal();
    }
}