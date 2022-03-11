using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Topo;
using DeBroglie.Models;
using System.Linq;
using System;

public class HexTileTiledTerrain : TiledTerrain
{

    public override void Initialize(List<GameObject> inputTiles, int width, int height)
    {
        base.Initialize(inputTiles, width, height);
        model = new AdjacentModel(DirectionSet.Hexagonal2d);
        topology = new GridTopology(DirectionSet.Hexagonal2d, width, height, false, false);
    }

    protected override void ComputeAdjacencies()
    {
        foreach (GameObject inputTile in inputTiles)
        {
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
    }

    protected override ITileConstraint[] BuildConstraints()
    {
        return new ITileConstraint[] { };
    }

    protected override void PlaceTiles(ITopoArray<DeBroglie.Tile> result)
    {
        int offset = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject source = result.Get(x, z).Value as GameObject;
                // Reverse z axis as DeBroglie doesn't use the same as Unity
                Vector3 newPosition = new Vector3(x * tileSize - offset, 0, height - z * tileSize * 0.867f);
                GameObject newTile = GameObject.Instantiate(source, newPosition, source.transform.rotation);
                newTile.transform.SetParent(transform, false);
                newTile.transform.DestroyImmediateAllChildren();
            }
            offset++;
        }
    }

}