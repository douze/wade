using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Topo;
using DeBroglie.Models;
using System.Linq;
using System;

public class TiledTerrain : MonoBehaviour
{

    private int width;
    private int height;
    private float tileSize;

    private List<GameObject> inputTiles;
    private List<GameObject> outputTiles;

    private AdjacentModel model;
    private GridTopology topology;

    /// <summary> Initialize the terrain from <c>inputTiles</c>.</summary>
    public void Initialize(List<GameObject> inputTiles, int width, int height)
    {
        this.inputTiles = inputTiles;
        this.width = width;
        this.height = height;
        topology = new GridTopology(width, height, false);
        tileSize = ValidateTileSize();
        transform.DestroyImmediateAllChildren();
    }

    /// <summary> Validate all the tile size by comparing the bound x and z.</summary>
    private float ValidateTileSize()
    {
        if (inputTiles.Count == 0) return 0.0f;

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
        return referenceBounds.size.x;
    }

    private void ComputeAdjacencies()
    {
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
    }

    private EdgedPathConstraint ComputePathConstraint()
    {
        DeBroglie.Tile[] mainPathTiles = inputTiles
            .FindAll(currentTile => currentTile.GetComponent<SquareTile>().mainPath)
            .Select(currentTile => new DeBroglie.Tile(currentTile))
            .ToArray();

        Dictionary<DeBroglie.Tile, ISet<Direction>> exits = new Dictionary<DeBroglie.Tile, ISet<Direction>>();
        foreach (DeBroglie.Tile mainPathTile in mainPathTiles)
        {
            ISet<Direction> directions = new HashSet<Direction>();
            // FIXME
            SquareTile squareTile = (mainPathTile.Value as GameObject).GetComponent<SquareTile>();
            if (squareTile.top == Tile.EdgeType.Path) directions.Add(Direction.YMinus);
            if (squareTile.bottom == Tile.EdgeType.Path) directions.Add(Direction.YPlus);
            if (squareTile.left == Tile.EdgeType.Path) directions.Add(Direction.XMinus);
            if (squareTile.right == Tile.EdgeType.Path) directions.Add(Direction.XPlus);
            exits.Add(mainPathTile, directions);
        }

        return new EdgedPathConstraint(exits);
    }

    private FixedTileConstraint[] ComputeFixedTileConstraint()
    {
        List<GameObject> entryPointTiles = inputTiles
            .FindAll(currentTile => currentTile.GetComponent<SquareTile>().entryPoint.active);
        List<GameObject> exitPointTiles = inputTiles
            .FindAll(currentTile => currentTile.GetComponent<SquareTile>().exitPoint.active);

        FixedTileConstraint entryFixedTileConstraint = new FixedTileConstraint
        {
            Tiles = entryPointTiles.Select(currentTile => new DeBroglie.Tile(currentTile)).ToArray(),
            // FIXME
            Point = new Point(entryPointTiles[0].GetComponent<SquareTile>().entryPoint.position.x, entryPointTiles[0].GetComponent<SquareTile>().entryPoint.position.y)
        };

        FixedTileConstraint exitFixedTileConstraint = new FixedTileConstraint
        {
            Tiles = exitPointTiles.Select(currentTile => new DeBroglie.Tile(currentTile)).ToArray(),
            // FIXME
            Point = new Point(exitPointTiles[0].GetComponent<SquareTile>().exitPoint.position.x, exitPointTiles[0].GetComponent<SquareTile>().exitPoint.position.y)
        };

        return new FixedTileConstraint[] { entryFixedTileConstraint, exitFixedTileConstraint };
    }

    public void ComputeFrequencies()
    {
        foreach (GameObject inputTile in inputTiles)
        {
            model.SetFrequency(new DeBroglie.Tile(inputTile), inputTile.GetComponent<Tile>().frequency);
        }
    }

    /// <summary> Generate a square grid using WFC algorithm and square tiles.</summary>
    public void GenerateWFCGrid(bool debugMode = false)
    {
        model = new AdjacentModel(DirectionSet.Cartesian2d);
        outputTiles.Clear();

        ComputeAdjacencies();
        EdgedPathConstraint pathConstraint = ComputePathConstraint();
        FixedTileConstraint[] fixedTileConstraints = ComputeFixedTileConstraint();
        ComputeFrequencies();

        TilePropagator propagator = new TilePropagator(model, topology, new TilePropagatorOptions
        {
            BackTrackDepth = 12,
            Constraints = new ITileConstraint[] { pathConstraint, fixedTileConstraints[0], fixedTileConstraints[1] }
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
                newTile.transform.SetParent(transform, false);
                newTile.transform.DestroyImmediateAllChildren();
                newTile.GetComponent<Tile>().position = new Vector2Int(x, z);
                outputTiles.Add(newTile);
             }
        }
    }

    public void UseDebugMaterial(Material originalMaterialToReplace, Material pathMaterial, Material fixedTileMaterial)
    {
       outputTiles.ForEach(tile => tile.GetComponent<Tile>().UseDebugMaterial(originalMaterialToReplace, pathMaterial, fixedTileMaterial, tile.GetComponent<Tile>().position));
    }

    public void UseNormalMaterial(Material originalMaterial)
    {
        outputTiles.ForEach(tile => tile.GetComponent<Tile>().UseNormalMaterial(originalMaterial));
    }
}
