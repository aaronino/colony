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

}
