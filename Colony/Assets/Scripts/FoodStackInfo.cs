using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class FoodStackInfo
{
    public Vector2Int Coordinates;
    public int Size;
    public GameObject Stack;
    public short ColorIndex;
    
    public FoodStackInfo(Vector2Int coords, int size, short colorIndex, GameObject stack)
    {
        Coordinates = coords;
        Size = size;
        Stack = stack;
        ColorIndex = colorIndex;
    }
    
    
}
