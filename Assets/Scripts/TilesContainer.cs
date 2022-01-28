using UnityEngine;

/// <summary>Container for tiles with auto placement and component management.</summary>
[ExecuteAlways]
public class TilesContainer : MonoBehaviour
{
    private void OnTransformChildrenChanged() {
        for (int i = 0 ; i < transform.childCount ; i++) {
            GameObject child = transform.GetChild(i).gameObject;
            child.transform.position = new Vector3(2 * i * child.GetComponent<MeshFilter>().sharedMesh.bounds.size.z + 1, 0, 0);
            if (child.GetComponent<SquareTile>() == null) {
                child.AddComponent<SquareTile>();
            }
        }
    }
}
