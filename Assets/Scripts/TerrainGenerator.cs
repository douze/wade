using UnityEngine;
using UnityEditor;

public class TerrainGenerator : MonoBehaviour
{

    public int width;
    public int height;
    public GameObject tiles;
    public GameObject output;

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
            // Clean
            while (terrainGenerator.output.transform.childCount > 0)
            {
                DestroyImmediate(terrainGenerator.output.transform.GetChild(0).gameObject);
            }

            // Generate
            int count = terrainGenerator.tiles.transform.childCount;
            int tileSize = 2;

            for (int j = 0; j < terrainGenerator.height; j++)
            {
                for (int i = 0; i < terrainGenerator.width; i++)
                {
                    GameObject tile = GameObject.Instantiate(terrainGenerator.tiles.transform.GetChild(Random.Range(0, count)).gameObject, terrainGenerator.output.transform);
                    tile.transform.position = new Vector3(i*tileSize, 0, j*tileSize);
                }
            }

        }
    }
}
