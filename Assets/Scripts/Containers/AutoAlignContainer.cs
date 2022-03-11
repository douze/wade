using UnityEngine;

/// <summary>Container for tiles with auto placement and component management.</summary>
[ExecuteAlways]
public class AutoAlignContainer<T> : MonoBehaviour
{
    private void OnTransformChildrenChanged() {
        float offset = 0.0f;
        for (int i = 0 ; i < transform.childCount ; i++) {
            GameObject child = transform.GetChild(i).gameObject;
            if (i > 0) {
                GameObject previousChild = transform.GetChild(i-1).gameObject;
                float previousPositionX = transform.GetChild(i-1).localPosition.x;
                float previousExtentX = previousChild.GetBoundingBoxExtent().x;
                float currentExtentX = child.GetBoundingBoxExtent().x;
                offset = previousPositionX + previousExtentX + currentExtentX + 1;
            }
            child.transform.localPosition = new Vector3(offset,0,0);
            if (child.GetComponent<T>() == null) {
                child.AddComponent(typeof(T));
            }
        }
    }
}
