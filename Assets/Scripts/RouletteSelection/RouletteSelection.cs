using System.Collections.Generic;
using System;

public class RouletteSelection
{
    public static float GetRandom(float min, float max)
    {
        return min + (UnityEngine.Random.value * (max - min));
    }
    public static T Roulette<T>(Dictionary<T, float> items)
    {
        float total = 0;
        foreach (var item in items)
        {
            total += item.Value;
        }
        float random = UnityEngine.Random.Range(0, total);

        foreach (var item in items)
        {
            random = random - item.Value;
            if (random <= 0)
            {
                return item.Key;
            }
        }
        return default(T);
    }
    public static List<T> Shuffle<T>(List<T> list, Action<T, T> onSwap = null)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            if (onSwap != null)
                onSwap(list[i], list[r]);
            T aux = list[i];
            list[i] = list[r];
            list[r] = aux;
        }
        return list;
    }
    public static T[] Shuffle<T>(T[] list, Action<T, T> onSwap = null)
    {
        for (int i = 0; i < list.Length; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Length);
            if (onSwap != null)
                onSwap(list[i], list[r]);
            T aux = list[i];
            list[i] = list[r];
            list[r] = aux;
        }
        return list;
    }
}
