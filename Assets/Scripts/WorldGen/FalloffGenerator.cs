using UnityEngine;
using System.Collections;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float) size; // Normalise coordinates
                float y = j / (float) size;

                float distToCentre = Vector2.Distance(new Vector2(x, y), Vector2.one * 0.5f) / Mathf.Sqrt(0.5f);

                map[i, j] = evaluate(distToCentre);
            }
        }

        return map;
    }

    private static float evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}