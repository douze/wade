using UnityEngine;

/// <summary>A <c>Prop</c> is an object used to decorate the terrain (tree, rock, building...).</summary>
public class Prop : MonoBehaviour
{
    // The weight attribute contribute to the total weight assigned to a tile
    public int weight = 1;

    [Header("Offsets")]
    [Tooltip("Final position = random(-positionOffset, positionOffset)")]
    public float positionOffset = 0f;
    [Tooltip("Final rotation = random(0, rotationOffset)")]
    public float rotationOffset = 0f;
    [Tooltip("Final scale = random(1-scaleOffset, 1)")]
    public float scaleOffset = 0f;
    
}
