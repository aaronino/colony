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

    public static void PushScent(this Dictionary<string, ScentInfo> scents, string name, int strength)
    {
        ScentInfo scent;
        var exists = scents.TryGetValue(name, out scent);

        if (!exists)
        {
            scent = new ScentInfo() {Name = name};
            scents.Add(name, scent);
        }

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
        return source.Where(x => x.NearColony.Distance == 0);
    }

    public static IEnumerable<HexInfo> Scented(this IEnumerable<HexInfo> source)
    {
        return source.Where(x => x.Scents.Any());
    }
}
