using UnityEngine;
using System;
using System.Collections.Generic;

namespace NGS.PolygonalCulling
{
    public static class FloatHelper
    {
        public static bool IsEqual(this float a, float b, float accuracy = 0.00001f)
        {
            return Mathf.Abs(a - b) <= accuracy;
        }
    }

    public static class ArrayHelper
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var current in enumerable)
                action(current);
        }

        public static void DistinctAdd<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        public static void DistinctAddRange<T>(this List<T> list, IEnumerable<T> items)
        {
            items.ForEach(i => list.DistinctAdd(i));
        }
    }
}
