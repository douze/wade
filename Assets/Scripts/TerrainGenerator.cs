using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Topo;
using DeBroglie.Models;
using System;
using System.Linq;

/// <summary>Generate a terrain using WFC.</summary>
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{

    [Header("Characteristics")]
    public int width;
    public int height;
    public bool hex = false;

    [Header("Nodes")]
    public GameObject inputNode;
    public GameObject outputNode;
    private List<GameObject> inputTiles;
    private float tileSize = 0.0f;
    private List<GameObject> outputTiles;

    [Header("Debug")]
    public bool debugMode = true;
    public Material originalMaterialToReplace;
    public Material pathTileMaterial;
    public Material fixedTilePaterial;

    public void Initialize()
    {
        inputTiles = Flatten(inputNode);
        if (inputTiles.Count == 0) throw new Exception("Input Node unassigned");
        ValidateTileSize();
        outputTiles = new List<GameObject>();
    }

    public void OnValidate()
    {
        if (debugMode)
        {
            inputTiles.ForEach(tile => tile.GetComponent<Tile>().UseDebugMaterial(originalMaterialToReplace, pathTileMaterial, fixedTilePaterial));
            outputTiles.ForEach(tile => tile.GetComponent<Tile>().UseDebugMaterial(originalMaterialToReplace, pathTileMaterial, fixedTilePaterial, tile.GetComponent<Tile>().position));
        }
        else
        {
            inputTiles.ForEach(tile => tile.GetComponent<Tile>().UseNormalMaterial(originalMaterialToReplace));
            outputTiles.ForEach(tile => tile.GetComponent<Tile>().UseNormalMaterial(originalMaterialToReplace));
        }
    }

    /// <summary>Remove all GameObject instances from the <c>outputNode</c>.</summary>
    public void CleanOuput()
    {
        outputNode.transform.DestroyImmediateAllChildren();
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

        float epsilon = 0.5f;
        Bounds referenceBounds = inputTiles[0].GetComponent<MeshFilter>().sharedMesh.bounds;
        foreach (GameObject inputTile in inputTiles)
        {
            Bounds bounds = inputTile.GetComponent<MeshFilter>().sharedMesh.bounds;
            if (Mathf.Abs(bounds.size.x - bounds.size.z) > epsilon || Mathf.Abs(bounds.size.x - referenceBounds.size.x) > epsilon)
            {
                throw new Exception("Invalid tile size for " + inputTile.name + " (" + bounds.size + " VS ref " + referenceBounds.size + " -- " + epsilon + ")");
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
                tile.transform.DestroyImmediateAllChildren();
            }
        }
    }

    /// <summary> Generate a square grid using WFC algorithm and square tiles.</summary>
    public void GenerateWFCGrid()
    {
        AdjacentModel model = new AdjacentModel(DirectionSet.Cartesian2d);
        GridTopology topology = new GridTopology(width, height, false);

        foreach (GameObject inputTile in inputTiles)
        {
            // Find opposite matchings
            DeBroglie.Tile[] matchingRight = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<SquareTile>().right == currentTile.GetComponent<SquareTile>().left)
                .Select(currentTile => new DeBroglie.Tile(currentTile))
                .ToArray();
            DeBroglie.Tile[] matchingBottom = inputTiles
                .FindAll(currentTile => inputTile.GetComponent<SquareTile>().bottom == currentTile.GetComponent<SquareTile>().top)
                .Select(currentTile => new DeBroglie.Tile(currentTile))
                .ToArray();

            DeBroglie.Tile tile = new DeBroglie.Tile(inputTile);
            model.AddAdjacency(new[] { tile }, matchingRight, Direction.XPlus);
            model.AddAdjacency(new[] { tile }, matchingBottom, Direction.YPlus);
        }

        model.SetUniformFrequency();

        // Constraints : main path
        DeBroglie.Tile[] mainPathTiles = inputTiles
            .FindAll(currentTile => currentTile.GetComponent<SquareTile>().mainPath)
            .Select(currentTile => new DeBroglie.Tile(currentTile))
            .ToArray();

        Dictionary<DeBroglie.Tile, ISet<Direction>> exits = new Dictionary<DeBroglie.Tile, ISet<Direction>>();
        foreach (DeBroglie.Tile mainPathTile in mainPathTiles)
        {
            ISet<Direction> directions = new HashSet<Direction>();
            SquareTile squareTile = (mainPathTile.Value as GameObject).GetComponent<SquareTile>();
            if (squareTile.top == Tile.EdgeType.Path) directions.Add(Direction.YMinus);
            if (squareTile.bottom == Tile.EdgeType.Path) directions.Add(Direction.YPlus);
            if (squareTile.left == Tile.EdgeType.Path) directions.Add(Direction.XMinus);
            if (squareTile.right == Tile.EdgeType.Path) directions.Add(Direction.XPlus);
            exits.Add(mainPathTile, directions);
        }

        EdgedPathConstraint pathConstraint = new EdgedPathConstraint(exits);

        // Constraints : fixed tiles
        List<GameObject> entryPointTiles = inputTiles
            .FindAll(currentTile => currentTile.GetComponent<SquareTile>().entryPoint.active);
        List<GameObject> exitPointTiles = inputTiles
            .FindAll(currentTile => currentTile.GetComponent<SquareTile>().exitPoint.active);

        FixedTileConstraint entryFixedTileConstraint = new FixedTileConstraint
        {
            Tiles = entryPointTiles.Select(currentTile => new DeBroglie.Tile(currentTile)).ToArray(),
            Point = new Point(entryPointTiles[0].GetComponent<SquareTile>().entryPoint.position.x, entryPointTiles[0].GetComponent<SquareTile>().entryPoint.position.y)
        };

        FixedTileConstraint exitFixedTileConstraint = new FixedTileConstraint
        {
            Tiles = exitPointTiles.Select(currentTile => new DeBroglie.Tile(currentTile)).ToArray(),
            Point = new Point(exitPointTiles[0].GetComponent<SquareTile>().exitPoint.position.x, exitPointTiles[0].GetComponent<SquareTile>().exitPoint.position.y)
        };

        // Frequencies
        model.SetFrequency(new DeBroglie.Tile(inputTiles[0]), 2.0);

        TilePropagator propagator = new TilePropagator(model, topology, new TilePropagatorOptions
        {
            BackTrackDepth = 12,
            Constraints = new ITileConstraint[] { pathConstraint, entryFixedTileConstraint, exitFixedTileConstraint }
        });

        DeBroglie.Resolution status = propagator.Run();
        if (status != DeBroglie.Resolution.Decided) throw new Exception(status.ToString());

        ITopoArray<DeBroglie.Tile> result = propagator.ToArray();
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject source = result.Get(x, z).Value as GameObject;
                // Reverse z axis as DeBroglie doesn't use the same as Unity
                Vector3 newPosition = new Vector3(x * tileSize, 0, height - z * tileSize);
                GameObject newTile = GameObject.Instantiate(source, newPosition, source.transform.rotation);
                newTile.transform.SetParent(outputNode.transform, false);
                newTile.transform.DestroyImmediateAllChildren();
                newTile.GetComponent<Tile>().position = new Vector2Int(x, z);
                outputTiles.Add(newTile);
                if (debugMode)
                {
                    newTile.GetComponent<Tile>().UseDebugMaterial(pathTileMaterial, fixedTilePaterial, newTile.GetComponent<Tile>().position);
                }
                else
                {
                    newTile.GetComponent<Tile>().UseNormalMaterial(originalMaterialToReplace);
                }
            }
        }
    }

    /// <summary> Generate a rombus grid using WFC algorithm and hex tiles.</summary>
    public void GenerateWFCGridWithHex()
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
            if (terrainGenerator.hex)
            {
                terrainGenerator.GenerateWFCGridWithHex();
            }
            else
            {
                terrainGenerator.GenerateWFCGrid();
            }
        }
    }
}
