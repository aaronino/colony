using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class Extensions
{
    public static T RandomElement<T>(this IEnumerable<T> source)
    {
        T current = default(T);
        int count = 0;
        foreach (T element in source)
        {
            if (UnityEngine.Random.Range(0, count) == 0)
            {
                current = element;
            }
            count++;
        }
        return current;
    }

    public static void PushScent(this ScentInfo scent, string name, int strength)
    {
        if (strength <= 0)
            return;

        scent.Name = name;

        if (strength > scent.Strength)
        {
            scent.Strength = strength;
            scent.State = ScentState.Holding;
        }
        else if (strength == scent.Strength)
        {
            scent.State = ScentState.Spreading;
        }
    }

    public static IEnumerable<HexInfo> Unexplored(this IEnumerable<HexInfo> source)
    {
        return source.Where(x => x.HomeInfo.Distance == 0);
    }

    public static IEnumerable<HexInfo> Scented(this IEnumerable<HexInfo> source)
    {
        return source.Where(x => x.FoodScent.Strength > 0);
    }

    public static IEnumerable<Vector2Int> AdjacentTo(this IEnumerable<Vector2Int> source, Vector2Int coords)
    {
        return source.Where(x => Vector2Int.Distance(x, coords) <= 1);
    }

    public static IEnumerable<Vector2Int> NonAdjacentTo(this IEnumerable<Vector2Int> source, Vector2Int coords)
    {
        return source.Where(x => Vector2Int.Distance(x, coords) > 1);
    }

    public static string FoodDirection(this HexInfo hex)
    {
        if (hex.FoodInfo.Distance == 0)
            return "?";

        var direction = string.Empty;
        if (hex.FoodInfo.Coordinates.y < hex.Coordinates.y)
        {
            direction += "n";
        }
        if (hex.FoodInfo.Coordinates.y > hex.Coordinates.y)
        {
            direction += "s";
        }
        if (hex.FoodInfo.Coordinates.x < hex.Coordinates.x)
        {
            direction += "w";
        }
        if (hex.FoodInfo.Coordinates.x > hex.Coordinates.x)
        {
            direction += "e";
        }

        return direction;
    }

    public static string HomeDirection(this HexInfo hex)
    {
        if (hex.HomeInfo.Distance == 0)
            return "?";

        var direction = string.Empty;
        if (hex.HomeInfo.Coordinates.y < hex.Coordinates.y)
        {
            direction += "n";
        }
        if (hex.HomeInfo.Coordinates.y > hex.Coordinates.y)
        {
            direction += "s";
        }
        if (hex.HomeInfo.Coordinates.x < hex.Coordinates.x)
        {
            direction += "w";
        }
        if (hex.HomeInfo.Coordinates.x > hex.Coordinates.x)
        {
            direction += "e";
        }

        return direction;
    }
}
