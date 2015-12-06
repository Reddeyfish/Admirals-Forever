using UnityEngine;
using System.Collections;

//resets data; intended to be used with the "EnableToResetData" prefab

[ExecuteInEditMode]
public class ResetData : MonoBehaviour {
    public void Start()
    {
        Reset();
    }

    //wipe the playerprefs data
    public void Reset()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Data Reset!");
    }
}
