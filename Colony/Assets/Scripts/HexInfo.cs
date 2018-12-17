using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class HexInfo {
    public Vector2Int Coordinates;
        
    public bool HasPellet;
    public bool HasFoodStack;
    public bool HasAnt;
    public bool IsColony;
    public bool IsRock;
    
    // chem trails
    public ChemInfo FoodInfo;
    public ChemInfo HomeInfo;
    public long LastTouched;

    // scents
    public ScentInfo FoodScent;

    // adjacent
    public List<Vector2Int> AdjacentSpaces;

    public HexInfo(Vector2Int coords) 
        : this(coords.x, coords.y)
    {
    }

    public HexInfo(int x, int y)
        : this ()
    {
        Coordinates = new Vector2Int(x, y);

        int offset = (y % 2 == 1) ? 0 : 1;
        AdjacentSpaces = new List<Vector2Int>() {
            new Vector2Int(x - offset, y - 1),
            new Vector2Int(x + 1 - offset, y - 1),
            new Vector2Int(x - 1, y),
            new Vector2Int(x + 1, y),
            new Vector2Int(x - offset, y + 1),
            new Vector2Int(x + 1 - offset, y + 1)
        };
    }

    public HexInfo()
    {
        HomeInfo = new ChemInfo();
        FoodInfo = new ChemInfo();
        FoodScent = new ScentInfo();
    }

    public bool HasFood
    {
        get { return HasPellet || HasFoodStack; }
    }

    public bool IsEmpty
    {
        get { return !HasFood && !HasAnt && IsPathable; }
    }

    public bool IsPathable
    {
        get { return !IsRock && !IsColony; }
    }

    public bool HasActiveScent
    {
        get { return FoodScent.Strength > 0 && FoodScent.State != ScentState.Holding; }
    }

    // Returns an empty HexInfo with coordinates provided
    public static HexInfo Empty(int x, int y)
    {
        return new HexInfo(x, y);

    }
}
