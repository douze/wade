using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareTile : MonoBehaviour
{
    public enum EdgeType
    {
        Grass,
        Path
    }

    public EdgeType top;
    public EdgeType right;
    public EdgeType bottom;
    public EdgeType left;

}
