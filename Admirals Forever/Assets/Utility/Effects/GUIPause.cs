using UnityEngine;
using System.Collections;

//static class for time-dilation effects and pausing

//also includes a non-static wrapper so that these functions can be linked via inspector to GUI buttons and stuff. If possible, just use the static functions

public static class Pause {

    private static float? currentTimeScale;

    private static bool paused = false;
	public static bool isPaused()
	{
		return paused;
	}

    private static bool frozen = false;
	public static bool isFrozen()
	{
		return frozen;
	}

	public static void pause(bool toPause = true)
    {
        if (toPause && !paused)
        {
            if (!frozen)
                currentTimeScale = Time.timeScale;
            Time.timeScale = 0;
            paused = true;
        }
        else if (!toPause && paused)
        {
            Time.timeScale = currentTimeScale ?? 1.0f;
            paused = false;
        }

        // maybe warnings if nothing will happen?
    }

    //inverse of Pause, basically.
    public static void unPause(bool toUnPause = true)
    {
        if (!toUnPause && !paused)
        {
            if (!frozen)
                currentTimeScale = Time.timeScale;
            Time.timeScale = 0;
            paused = true;
        }
        else if (toUnPause && paused)
        {
            Time.timeScale = currentTimeScale ?? 1.0f;
            paused = false;
        }
    }

    //freeze time for a small amount in order to emphasize an impact or effect
    public static IEnumerator FreezeRoutine(float durationRealSeconds, float timeScale = 0f)
    {
        if (frozen)
            yield break;
        frozen = true;

        if(!paused)
            currentTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
        float pauseEndTime = Time.realtimeSinceStartup + durationRealSeconds;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return 0;
        }
        frozen = false;
        Time.timeScale = currentTimeScale ?? 1.0f;
    }

	public static void Freeze(MonoBehaviour callingScript, float durationRealSeconds, float timeScale = 0f) //wrapper for FreezeRoutine so that we don't need to call StartCoroutine()
	{
		callingScript.StartCoroutine(FreezeRoutine(durationRealSeconds, timeScale));
	}
}

//non-static wrapper so that GUI buttons can be easily linked up via inspector
public class GUIPause : MonoBehaviour
{
    public void pause(bool toPause = true) //instanced (not static) wrapper so that UI event systems can call it
    {
        Pause.pause(toPause);
    }


    public void unPause(bool toUnPause = true) //instanced (not static) wrapper so that UI event systems can call it
    {
        Pause.unPause(toUnPause);
    }
}