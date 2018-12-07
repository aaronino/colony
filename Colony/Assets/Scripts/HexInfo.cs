using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class HexInfo {
    public Vector2Int Coordinates;
        
    public bool HasFood;
    public bool HasAnt;
    public bool IsColony;

    // chem trails
    public ChemInfo NearResource;
    public ChemInfo NearEnemy;
    public ChemInfo NearColony;

    // scents
    public List<ScentInfo> Scents;

    public HexInfo(Vector2Int coords, bool hasFood = false, bool hasAnt = false, bool isColony = false) 
        : this(coords.x, coords.y, hasFood, hasAnt, isColony)
    {
    }

    public HexInfo(int x, int y, bool hasFood = false, bool hasAnt = false, bool isColony = false)
        : this ()
    {
        Coordinates = new Vector2Int(x, y);
        HasFood = hasFood;
        HasAnt = hasAnt;
        IsColony = isColony;
    }

    public HexInfo()
    {
        Scents = new List<ScentInfo>();
        NearColony = new ChemInfo();
    }

    public bool IsEmpty {
        get { return !HasFood && !HasAnt && !IsColony; }
    }

    // Returns an empty HexInfo with coordinates provided
    public static HexInfo Empty(int x, int y)
    {
        return new HexInfo(x, y);

    }
}
