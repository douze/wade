using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DeBroglie;
using DeBroglie.Topo;
using DeBroglie.Models;
using System;
using System.Linq;

/// <summary>Generate a terrain using WFC.</summary>

public class TerrainGenerator : MonoBehaviour
{

    public int width;
    public int height;

    public GameObject inputNode;
    public GameObject outputNode;
    private List<GameObject> inputTiles;
    private float tileSize = 0.0f;

    public void Initialize()
    {
        inputTiles = Flatten(inputNode);
        if (inputTiles.Count == 0) throw new Exception("Input Node unassigned");
        ValidateTileSize();
    }

    /// <summary>Remove all GameObject instances from the <c>outputNode</c>.</summary>
    public void CleanOuput()
    {
        while (outputNode.transform.childCount > 0)
        {
            DestroyImmediate(outputNode.transform.GetChild(0).gameObject);
        }
    }

    /// <summary> Flatten the <c>inputNode</c> GameObject (arranged by mesh) to a single list of GameObject.</summary>
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

    /// <summary> Validate all the tile size by comparing the bound x and z.</summary>
    private void ValidateTileSize()
    {
        if (inputTiles.Count == 0) return;

        float epsilon = 0.00001f;
        Bounds referenceBounds = inputTiles[0].GetComponent<MeshFilter>().sharedMesh.bounds;
        foreach(GameObject inputTile in inputTiles)
        {
            Bounds bounds = inputTile.GetComponent<MeshFilter>().sharedMesh.bounds;
            if (Mathf.Abs(bounds.size.x - bounds.size.z) > epsilon || Mathf.Abs(bounds.size.x - referenceBounds.size.x) > epsilon) {
                throw new Exception("Invalid tile size for " + inputTile.name + " (" + bounds.size + " VS ref " + referenceBounds.size + " -- " + Mathf.Epsilon + ")");
            } 
        }
        tileSize = referenceBounds.size.x;
    }

    /// <summary> Generate a square grid using random tiles.</summary>
    /// <remarks> For testing purpose only. </remarks>
    public void GenerateRandomGrid()
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                GameObject tile = GameObject.Instantiate(inputTiles[UnityEngine.Random.Range(0, inputTiles.Count)], outputNode.transform);
                tile.transform.position = new Vector3(i * tileSize, 0, j * tileSize);

                while (tile.transform.childCount > 0)
                {
                    DestroyImmediate(tile.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    /// <summary> Generate a square grid using WFC algorithm.</summary>
    public void GenerateWFCGrid()
    {
        AdjacentModel model = new AdjacentModel(DirectionSet.Cartesian2d);
        GridTopology topology = new GridTopology(width, height, false);

        foreach (GameObject inputTile in inputTiles)
        {
            // Find opposite matchings
            Tile[] matchingRight = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<SquareTile>().right == currentTile.GetComponent<SquareTile>().left)
                .Select(currentTile => new Tile(currentTile))
                .ToArray();
            Tile[] matchingBottom = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<SquareTile>().bottom == currentTile.GetComponent<SquareTile>().top)
                .Select(currentTile => new Tile(currentTile))
                .ToArray();

            Tile tile = new Tile(inputTile);
            model.AddAdjacency(new[] { tile }, matchingRight, Direction.XPlus);
            model.AddAdjacency(new[] { tile }, matchingBottom, Direction.YPlus);
        }

        model.SetUniformFrequency();

        TilePropagator propagator = new TilePropagator(model, topology, new TilePropagatorOptions { BackTrackDepth = -1 });

        var status = propagator.Run();
        if (status != DeBroglie.Resolution.Decided) throw new Exception("Undecided");

        var outputres = propagator.ToArray();
        for (var z = 0; z < height; z++)
        {
            for (var x = 0; x < width; x++)
            {
                GameObject newTile = GameObject.Instantiate(outputres.Get(x, z).Value as GameObject, outputNode.transform);
                newTile.transform.position = new Vector3(x * tileSize, 0, height - z * tileSize); // Reverse z axis as DeBroglie doesn't use the same as Unity
                while (newTile.transform.childCount > 0)
                {
                    DestroyImmediate(newTile.transform.GetChild(0).gameObject);
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
            terrainGenerator.Initialize();
            terrainGenerator.CleanOuput();
            terrainGenerator.GenerateWFCGrid();
        }
    }
}
