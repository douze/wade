using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HexTile : Tile
{
    //    / \  5 6
    //    | |  4 1
    //    \ /  3 2

    public EdgeType one;
    public EdgeType two;
    public EdgeType three;
    public EdgeType four;
    public EdgeType five;
    public EdgeType six;

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
            newRotation *= Quaternion.Euler(0, i*60, 0);
            GameObject child = GameObject.Instantiate(gameObject, newPosition, newRotation);
            // Swap edges
            HexTile tile = child.GetComponent<HexTile>();
            for (int j = 0 ; j < i ; j++)
            {
                EdgeType saveOne = tile.one;
                tile.one = tile.six;
                tile.six = tile.five;
                tile.five = tile.four;
                tile.four = tile.three; 
                tile.three = tile.two;
                tile.two = saveOne;               
            }
            children.Add(child);
        }
        foreach(GameObject child in children)
        {
            child.transform.SetParent(transform);
        }
    }

}


[CustomEditor(typeof(HexTile))]
public class HexTile_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexTile hexTile = (HexTile)target;

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Variations");
        if (GUILayout.Button("1"))
        {
            hexTile.GenerateVariation(0);
        }
        if (GUILayout.Button("2"))
        {
            hexTile.GenerateVariation(1);
        }
        if (GUILayout.Button("3"))
        {
            hexTile.GenerateVariation(2);
        }
        if (GUILayout.Button("4"))
        {
            hexTile.GenerateVariation(3);
        }
        if (GUILayout.Button("5"))
        {
            hexTile.GenerateVariation(4);
        }
        if (GUILayout.Button("6"))
        {
            hexTile.GenerateVariation(5);
        }
        GUILayout.EndHorizontal();
    }
}

