using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Topo;
using DeBroglie.Models;
using System.Linq;
using System;

public class SquareTileTiledTerrain : TiledTerrain
{

    public override void Initialize(List<GameObject> inputTiles, int width, int height)
    {
        base.Initialize(inputTiles, width, height);
        model = new AdjacentModel(DirectionSet.Cartesian2d);
        topology = new GridTopology(width, height, false);
    }

    protected override void ComputeAdjacencies()
    {
        foreach (GameObject inputTile in inputTiles)
        {
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

    /// <summary> Compute the path constraints, using tiles marked as <c>mainPath</c>.</summary>
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

    /// <summary> Compute the fixed tile constraints, using tiles marked as <c>entrePoint</c> and <c>exitPoint</c>.</summary>
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

    protected override ITileConstraint[] BuildConstraints()
    {
        EdgedPathConstraint pathConstraint = ComputePathConstraint();
        FixedTileConstraint[] fixedTileConstraints = ComputeFixedTileConstraint();
        return new ITileConstraint[] { pathConstraint, fixedTileConstraints[0], fixedTileConstraints[1] };
    }

    protected override void PlaceTiles(ITopoArray<DeBroglie.Tile> result)
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject source = result.Get(x, z).Value as GameObject;
                // Reverse z axis as DeBroglie doesn't use the same as Unity
                Vector3 newPosition = new Vector3(x * tileSize, 0, height - z * tileSize);
                GameObject newTile = GameObject.Instantiate(source, newPosition, source.transform.rotation);
                newTile.transform.SetParent(transform, false);
                if (newTile.transform.childCount > 0 && newTile.transform.GetChild(0).GetComponent<Tile>() != null) newTile.transform.DestroyImmediateAllChildren();
                newTile.GetComponent<Tile>().position = new Vector2Int(x, z);
                outputTiles.Add(newTile);
                PlaceProps(newTile.GetComponent<Tile>());
            }
        }
    }

}