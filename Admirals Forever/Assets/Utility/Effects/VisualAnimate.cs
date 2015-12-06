using UnityEngine;
using System.Collections;

public class VisualAnimate : MonoBehaviour {

    //places the specified FX shader on the specified target and animates it
    public float fxTime;
    public Material fxMat;
    public Renderer[] targets;
	// Update is called once per frame

    void Awake()
    {
        fxMat = Instantiate(fxMat);
    }

	public void DoFX () {
        Material[] previousMats = new Material[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            previousMats[i] = targets[i].material;
            targets[i].material = fxMat;
        }
        Callback.DoLerp((float t) => fxMat.SetFloat(Tags.ShaderParams.cutoff, t), fxTime, this).FollowedBy(() => 
        {
            for (int i = 0; i < targets.Length; i++)
                targets[i].material = previousMats[i];
        }, this);
	}
}
