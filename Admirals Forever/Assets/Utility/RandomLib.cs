using UnityEngine;
using System.Collections;

//a small class to supplement Unity's Random class

public static class RandomLib {

    public static float RandFloatRange(float midpoint, float variance)
    {
        return midpoint + (variance * Random.value);
    }

    public static T[] Shuffle<T>(this T[] originalArray)
    {
        //Fisher-Yates algorithm
        for (int i = 0; i < originalArray.Length; i++)
        {
            T temp = originalArray[i];
            int swapIndex = Random.Range(i, originalArray.Length);
            originalArray[i] = originalArray[swapIndex];
            originalArray[swapIndex] = temp;
        }
        return originalArray;
    }
}
