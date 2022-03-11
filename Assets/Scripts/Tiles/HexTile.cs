using UnityEngine;

/// <summary>Represents a tile with a square shape.</summary>
public class HexTile : Tile
{
    //    / \  5 6
    //    | |  4 1
    //    \ /  3 2

    [Header("Edges")]
    public EdgeType one;
    public EdgeType two;
    public EdgeType three;
    public EdgeType four;
    public EdgeType five;
    public EdgeType six;

    public HexTile()
    {
        maxNumberOfVariations = 6;
    }

    protected override Quaternion GetRotation(int variation)
    {
        return Quaternion.Euler(0, variation * 60, 0);
    }

    protected override void SwapEdges(GameObject child, int variation)
    {
        HexTile tile = child.GetComponent<HexTile>();
        for (int i = 0 ; i < variation ; i++)
        {
            EdgeType saveOne = tile.one;
            tile.one = tile.six;
            tile.six = tile.five;
            tile.five = tile.four;
            tile.four = tile.three; 
            tile.three = tile.two;
            tile.two = saveOne;               
        }
    }
}
