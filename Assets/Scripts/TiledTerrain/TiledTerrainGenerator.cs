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

    [Header("Nodes")]
    public GameObject inputNode;
    public TiledTerrain outputTerrain;
    public GameObject inputProps;

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
        List<GameObject> Flatten(GameObject root, int depth)
        {
            List<GameObject> flattenTiles = new List<GameObject>();
            foreach (Transform child in root.transform)
            {
                flattenTiles.Add(child.gameObject);
                if (depth < 1) flattenTiles.AddRange(Flatten(child.gameObject, depth + 1));
            }
            return flattenTiles;
        }
        return Flatten(inputNode, 0);
    }
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

            terrainGenerator.debugMode = false;
            terrain.Initialize(terrainGenerator.GetInputTiles(), terrainGenerator.width, terrainGenerator.height);
            terrain.Generate();
        }
    }
}
