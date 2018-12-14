using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class HexInfo {
    public Vector2Int Coordinates;
        
    public bool HasFood;
    public bool HasFoodStack;
    public bool HasAnt;
    public bool IsColony;

    // chem trails
    public ChemInfo NearFood;
    public ChemInfo NearEnemy;
    public ChemInfo NearColony;

    // scents
    public Dictionary<string, ScentInfo> Scents;

    public HexInfo(Vector2Int coords) 
        : this(coords.x, coords.y)
    {
    }

    public HexInfo(int x, int y)
        : this ()
    {
        Coordinates = new Vector2Int(x, y);
    }

    public HexInfo()
    {
        Scents = new Dictionary<string, ScentInfo>();
        NearColony = new ChemInfo();
        NearFood = new ChemInfo();
        NearEnemy = new ChemInfo();
    }

    public bool IsEmpty {
        get { return !HasFood && !HasAnt && !IsColony; }
    }

    public bool HasActiveScent
    {
        get { return Scents.Any(x => x.Value.State != ScentState.Holding); }
    }

    public ScentInfo GetNextActiveScent()
    {
        return Scents.FirstOrDefault(x => x.Value.State != ScentState.Holding).Value;
    }

    public int GetAntScent()
    {
        return Scents.Where(x => x.Value.Name == "ant").Sum(x => x.Value.Strength);
    }

    public int GetFoodScent()
    {
        return Scents.Where(x => x.Value.Name == "food").Sum(x => x.Value.Strength);
    }

    public int GetInterest()
    {
        var interest = 100;

        if (NearColony.Distance > 0)
        {
            interest -= 10;
        }
        
        interest -= GetAntScent();

        if (NearFood.Distance > 0)
        {
            interest -= 10;
        }

        interest += GetFoodScent();

        return interest;
    }

    // Returns an empty HexInfo with coordinates provided
    public static HexInfo Empty(int x, int y)
    {
        return new HexInfo(x, y);

    }
}
