using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class HexInfo {
    public int X;
    public int Y;
        
    public bool HasFood;
    public bool HasAnt;
    public bool IsColony;
    
    public HexInfo(int x, int y, bool hasFood = false, bool hasAnt = false, bool isColony = false)
    {
        X = x;
        Y = y;
        HasFood = hasFood;
        HasAnt = hasAnt;
        IsColony = isColony;
    }

    public bool IsEmpty {
        get {
            if (HasFood || HasAnt || IsColony)
                return false;
            else
                return true;
        }
    }

    // Returns an empty HexInfo with coordinates provided
    public static HexInfo Empty(int x, int y)
    {
        return new HexInfo(x, y);

    }
}
