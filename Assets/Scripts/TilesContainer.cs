using UnityEngine;
using System;

/// <summary>Container for tiles with auto placement and component management.</summary>
[ExecuteAlways]
public class TilesContainer : MonoBehaviour
{
    private void OnTransformChildrenChanged() {
        Type tileType = gameObject.GetComponentInParent<TerrainGenerator>().hex ? typeof(HexTile) : typeof(SquareTile);
        for (int i = 0 ; i < transform.childCount ; i++) {
            GameObject child = transform.GetChild(i).gameObject;
            child.transform.localPosition = new Vector3(2 * i * child.GetComponent<MeshFilter>().sharedMesh.bounds.size.z + 1, 0, 0);
            if (child.GetComponent(tileType) == null) {
                child.AddComponent(tileType);
            }
        }
    }
}
