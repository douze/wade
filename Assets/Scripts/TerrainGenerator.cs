using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>Generate a terrain using WFC.</summary>

public class TerrainGenerator : MonoBehaviour
{

    public int width;
    public int height;
    public GameObject tiles;
    public GameObject output;

    /// <summary>Remove all GameObject instances from the ouput node.</summary>
    public void CleanOuput()
    {
        while (output.transform.childCount > 0)
        {
            DestroyImmediate(output.transform.GetChild(0).gameObject);
        }
    }


    /// <summary> Flatten the input tiles (arranged by mehs) to a single list of tiles.</summary>
    private List<GameObject> Flatten(GameObject root)
    {
        List<GameObject> flattenTiles = new List<GameObject>();
        foreach (Transform child in root.transform)
        {
            flattenTiles.Add(child.gameObject);
            flattenTiles.AddRange(Flatten(child.gameObject));
        }
        return flattenTiles;
    }

    /// <summary> Generate a square grid using random tiles.</summary>
    /// <remarks> For testing purpose only. </remarks>
    public void GenerateRandomGrid()
    {
        List<GameObject> flattenTiles = Flatten(tiles);
        int tileSize = 2;
        
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                GameObject tile = GameObject.Instantiate(flattenTiles[Random.Range(0, flattenTiles.Count)], output.transform);
                tile.transform.position = new Vector3(i * tileSize, 0, j * tileSize);

                while (tile.transform.childCount > 0)
                {
                    DestroyImmediate(tile.transform.GetChild(0).gameObject);
                }
            }
        }
    }

}

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGenerator_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainGenerator terrainGenerator = (TerrainGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            terrainGenerator.CleanOuput();
            terrainGenerator.GenerateRandomGrid();
        }
    }
}
