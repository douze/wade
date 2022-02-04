using UnityEngine;

/// <summary>Representes a tile with a square shape.</summary>
public class SquareTile : Tile
{

    [Header("Edges")]
    public EdgeType top;
    public EdgeType right;
    public EdgeType bottom;
    public EdgeType left;

    public SquareTile()
    {
        maxNumberOfVariations = 4;
    }

    protected override Quaternion GetRotation(int variation)
    {
        return Quaternion.Euler(0, variation * 90, 0);
    }

    protected override void SwapEdges(GameObject child, int variation)
    {
        SquareTile tile = child.GetComponent<SquareTile>();
        for (int i = 0; i < variation; i++)
        {
            EdgeType saveRight = tile.right;
            tile.right = tile.top;
            tile.top = tile.left;
            tile.left = tile.bottom;
            tile.bottom = saveRight;
        }
    }
}
