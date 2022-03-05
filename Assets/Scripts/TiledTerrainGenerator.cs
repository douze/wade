using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>Generate a terrain using WFC.</summary>
[ExecuteInEditMode]
public class TiledTerrainGenerator : MonoBehaviour
{

    [Header("Characteristics")]
    public int width;
    public int height;
    public bool hex = false;

    [Header("Nodes")]
    public GameObject inputNode;
    public TiledTerrain outputTerrain;

    [Header("Debug")]
    public bool debugMode = true;
    public Material originalMaterialToReplace;
    public Material pathTileMaterial;
    public Material fixedTileMaterial;

    public void OnValidate()
    {
        if (debugMode)
        {
            outputTerrain.UseDebugMaterial(originalMaterialToReplace, pathTileMaterial, fixedTileMaterial);
        } 
        else
        {
            outputTerrain.UseNormalMaterial(originalMaterialToReplace);   
        }
    }

    /// <summary> Return the input tiles, flattening the <c>inputNode</c> GameObject (arranged by mesh) to a single list of GameObject.</summary>
    public List<GameObject> GetInputTiles()
    {
        List<GameObject> Flatten(GameObject root)
        {
            List<GameObject> flattenTiles = new List<GameObject>();
            foreach (Transform child in root.transform)
            {
                flattenTiles.Add(child.gameObject);
                flattenTiles.AddRange(Flatten(child.gameObject));
            }
            return flattenTiles;
        }
        return Flatten(inputNode);
    }

    /// <summary> Generate a rombus grid using WFC algorithm and hex tiles.</summary>
    /*public void GenerateWFCGridWithHex()
    {
        AdjacentModel model = new AdjacentModel(DirectionSet.Hexagonal2d);
        GridTopology topology = new GridTopology(DirectionSet.Hexagonal2d, width, height, false, false);

        foreach (GameObject inputTile in inputTiles)
        {
            // Find opposite matchings
            DeBroglie.Tile[] matchingOneFour = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<HexTile>().one == currentTile.GetComponent<HexTile>().four)
                .Select(currentTile => new DeBroglie.Tile(currentTile))
                .ToArray();
            DeBroglie.Tile[] matchingTwoFive = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<HexTile>().two == currentTile.GetComponent<HexTile>().five)
                .Select(currentTile => new DeBroglie.Tile(currentTile))
                .ToArray();
            DeBroglie.Tile[] matchingThreeSix = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<HexTile>().three == currentTile.GetComponent<HexTile>().six)
                .Select(currentTile => new DeBroglie.Tile(currentTile))
                .ToArray();

            DeBroglie.Tile tile = new DeBroglie.Tile(inputTile);

            model.AddAdjacency(new[] { tile }, matchingOneFour, 1, 0, 0);
            model.AddAdjacency(new[] { tile }, matchingTwoFive, 1, 1, 0);
            model.AddAdjacency(new[] { tile }, matchingThreeSix, 0, 1, 0);
        }

        model.SetUniformFrequency();

        TilePropagator propagator = new TilePropagator(model, topology, new TilePropagatorOptions { BackTrackDepth = -1 });

        DeBroglie.Resolution status = propagator.Run();
        if (status != DeBroglie.Resolution.Decided) throw new Exception("Undecided");

        ITopoArray<DeBroglie.Tile> result = propagator.ToArray();
        int offset = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject source = result.Get(x, z).Value as GameObject;
                // Reverse z axis as DeBroglie doesn't use the same as Unity
                Vector3 newPosition = new Vector3(x * tileSize - offset, 0, height - z * tileSize * 0.867f);
                GameObject newTile = GameObject.Instantiate(source, newPosition, source.transform.rotation);
                newTile.transform.SetParent(outputNode.transform, false);
                newTile.transform.DestroyImmediateAllChildren();
            }
            offset++;
        }
    }*/

}

[CustomEditor(typeof(TiledTerrainGenerator))]
public class TiledTerrainGenerator_Inspector : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TiledTerrainGenerator terrainGenerator = (TiledTerrainGenerator)target;
        TiledTerrain terrain = terrainGenerator.outputTerrain;
        if (GUILayout.Button("Generate"))
        {
            /*terrainGenerator.Initialize();
            terrainGenerator.CleanOuput();
            if (terrainGenerator.hex)
            {
                terrainGenerator.GenerateWFCGridWithHex();
            }
            else
            {
                terrainGenerator.GenerateWFCGrid();
            }*/
            if (!terrainGenerator.hex)
            {
                terrainGenerator.debugMode = false;
                terrain.Initialize(terrainGenerator.GetInputTiles(), terrainGenerator.width, terrainGenerator.height);
                terrain.GenerateWFCGrid();
            }
        }
    }
}
