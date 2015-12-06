using UnityEngine;
using System.Collections;

//a class to assist with formatting

//makeReadable(int number) converts large numbers to use symbols (e.g. 2k instead of 2,000)

//mousePosInWorld() returns the world-position of the cursor at a Z-depth of zero
// TODO : (I might want to add more options and allow custom Z-depths to be used)

public static class Format{

    //transform it into 2k, 3m, etc.
    public static string makeReadable(int number) //int, so no decimals (YAY!)
    {
        int level = 0;
        while (Mathf.Abs(number) >= Mathf.Pow(1000, level+1)) level++;
        string result = number.ToString();
        if (level > 0)
            result = result.Substring(0, result.Length - level * 3) + levelToSuffix(level);
        //todo: commas
        return result;
    }

    private static string levelToSuffix(int level)
    {
        switch(level)
        {
            case 1: return "K";
            case 2: return "M";
            case 3: return "B";

            //you must be making an incremental game if you get here
                //no performance cost, so might as well add them
            case 4: return "T";
            case 5: return "Qa"; //Quadrillion
            case 6: return "Qi"; //Quintillion
            case 7: return "Sx"; //Sextillion
            case 8: return "Sp"; //Septillion
            case 9: return "Oc"; //Octillion
            case 10: return "No"; //Nonillion
            case 11: return "De"; //Decillion
            case 12: return "Un"; //Undecillion
            case 13: return "Du"; //Duodecillion
            case 14: return "Te"; //Tredecillion
            default: return "";
        }
    }
    public static Vector3 mousePosInWorld()
    {
        return mousePosInWorld(Camera.main.transform); //I don't want to use default parameters, because that would involve some extra computation through null-coalescing (the ?? thing)
    }

    public static Vector3 mousePosInWorld(Transform cameraTransform)
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = -cameraTransform.position.z;
        return screenPoint.toWorldPoint();
    }

    public static string formatMilliseconds(float numSeconds) //precision  = 0 means seconds, = 1 means milliseconds, and so on
    {
        System.Text.StringBuilder result = new System.Text.StringBuilder(Mathf.FloorToInt(numSeconds / 60f).ToString("00"));
        result.Append(':');

        numSeconds = Mathf.Abs(numSeconds % 60f);
        result.Append(((int)numSeconds).ToString("00"));
        result.Append(':');
        numSeconds = (numSeconds % 1f) / 0.001f;
        result.Append(((int)numSeconds).ToString("000"));
        return result.ToString();
    }
}