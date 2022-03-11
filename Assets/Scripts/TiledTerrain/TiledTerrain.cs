using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Topo;
using DeBroglie.Models;
using System.Linq;
using System;

/// <summary>A <c>TiledTerrain</c> is a grid of tiles.</summary>
public abstract class TiledTerrain : MonoBehaviour
{

    protected int width;
    protected int height;
    protected float tileSize;

    protected List<GameObject> inputTiles = new List<GameObject>();
    protected List<GameObject> outputTiles = new List<GameObject>();

    protected AdjacentModel model;
    protected GridTopology topology;

    /// <summary> Initialize the terrain from <c>inputTiles</c>.</summary>
    public virtual void Initialize(List<GameObject> inputTiles, int width, int height)
    {
        this.inputTiles = inputTiles;
        this.width = width;
        this.height = height;
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

    /// <summary> Compute the tile adjacencies, using opposite matchings (top--bottom, left--right).</summary>
    protected abstract void ComputeAdjacencies();

    /// <summary> Build the tile constrinats for WFC algorithm.</summary>
    protected abstract ITileConstraint[] BuildConstraints();

    /// <summary> Compute the tile frequencies, using <c>frequency</c> attribute.</summary>
    private void ComputeFrequencies()
    {
        foreach (GameObject inputTile in inputTiles)
        {
            model.SetFrequency(new DeBroglie.Tile(inputTile), inputTile.GetComponent<Tile>().frequency);
        }
    }

    /// <summary> Place tiles on the grid, using WFC <c>result</c>.</summary>
    protected abstract void PlaceTiles(ITopoArray<DeBroglie.Tile> result);

    /// <summary> Generate a grid using WFC algorithm.</summary>
    public void Generate()
    {
        outputTiles.Clear();

        ComputeAdjacencies();
        ComputeFrequencies();

        TilePropagator propagator = new TilePropagator(model, topology, new TilePropagatorOptions
        {
            BackTrackDepth = 12, // hardcoded -- I prefer contradiction over long run
            Constraints = BuildConstraints()
        });

        DeBroglie.Resolution status = propagator.Run();
        if (status != DeBroglie.Resolution.Decided) throw new Exception(status.ToString());

        ITopoArray<DeBroglie.Tile> result = propagator.ToArray();
        PlaceTiles(result);
    }

    /// <summary> Place props on the <c>tile</c>, while watching <c>maxPropsWeight</c> attribute.</summary>
    protected void PlaceProps(Tile tile)
    {
        TiledTerrainGenerator terrainGenerator = GetComponentInParent<TiledTerrainGenerator>();
        if (!tile.GetComponent<Tile>().trees || terrainGenerator == null) return;

        System.Random random = new System.Random();
        int sumPropsWeight = 0;

        while (sumPropsWeight < tile.maxPropsWeight)
        {
            GameObject inputProp = terrainGenerator.inputProps.GetAllChildren()
                .Where(currentProp => currentProp.GetComponent<Prop>().weight + sumPropsWeight <= tile.maxPropsWeight)
                .OrderBy(x => random.Next())
                .DefaultIfEmpty(null)
                .First();
            if (inputProp == null) return;

            Prop prop = inputProp.GetComponent<Prop>();
            sumPropsWeight += prop.weight;

            GameObject propInstance = GameObject.Instantiate(inputProp, tile.transform);
            float randomScale = UnityEngine.Random.Range(1.0f - prop.scaleOffset, 1.0f);
            propInstance.transform.localPosition = new Vector3(UnityEngine.Random.Range(-prop.positionOffset, prop.positionOffset), 1f, UnityEngine.Random.Range(-prop.positionOffset, prop.positionOffset));
            propInstance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            propInstance.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, prop.rotationOffset), 0f);
        }
    }

    /// <summary>Use the <c>pathMaterial</c> or <c>fixedTileMaterial</c> instead of the <c>originalMaterialToReplace</c>.</summary>
    public void UseDebugMaterial(Material originalMaterialToReplace, Material pathMaterial, Material fixedTileMaterial)
    {
        if (outputTiles != null)
            outputTiles.ForEach(tile => tile.GetComponent<Tile>().UseDebugMaterial(originalMaterialToReplace, pathMaterial, fixedTileMaterial, tile.GetComponent<Tile>().position));
    }

    /// <summary>Revert to the <c>originalMaterial</c>.</summary>
    public void UseNormalMaterial(Material originalMaterial)
    {
        if (outputTiles != null)
            outputTiles.ForEach(tile => tile.GetComponent<Tile>().UseNormalMaterial(originalMaterial));
    }
}
