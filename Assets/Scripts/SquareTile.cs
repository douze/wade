using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>Representes a tile with a square shape.</summary>
public class SquareTile : MonoBehaviour
{
    public enum EdgeType
    {
        Grass,
        Path
    }

    public EdgeType top;
    public EdgeType right;
    public EdgeType bottom;
    public EdgeType left;

    /// <summary>Generate variations of the current tile: new position, rotation and edges.</summary>
    public void GenerateVariation(int variations)
    {
        transform.DestroyImmediateAllChildren();
        List<GameObject> children = new List<GameObject>();
        for (int i = 1 ; i <= variations ; i++)
        {
            // Offset position
            Vector3 newPosition = transform.position;
            newPosition.z -= i * (GetComponent<MeshFilter>().sharedMesh.bounds.size.z + 1);
            // Rotate
            Quaternion newRotation = transform.rotation;
            newRotation *= Quaternion.Euler(0, i*90, 0);
            GameObject child = GameObject.Instantiate(gameObject, newPosition, newRotation);
            // Swap edges
            SquareTile tile = child.GetComponent<SquareTile>();
            for (int j = 0 ; j < i ; j++)
            {
                EdgeType saveRight = tile.right;
                tile.right = tile.top;
                tile.top = tile.left;
                tile.left = tile.bottom;
                tile.bottom = saveRight;                
            }
            children.Add(child);
        }
        foreach(GameObject child in children)
        {
            child.transform.SetParent(transform);
        }
    }

}

[CustomEditor(typeof(SquareTile))]
public class SquareTile_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SquareTile squareTile = (SquareTile)target;

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Variations");
        if (GUILayout.Button("1"))
        {
            squareTile.GenerateVariation(0);
        }
        if (GUILayout.Button("2"))
        {
            squareTile.GenerateVariation(1);
        }
        if (GUILayout.Button("4"))
        {
            squareTile.GenerateVariation(3);
        }
        GUILayout.EndHorizontal();
    }
}
