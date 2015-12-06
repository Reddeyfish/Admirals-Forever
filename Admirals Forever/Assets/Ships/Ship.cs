using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour {

    [CanBeDefaultOrNull]
    [SerializeField]
    protected int side; //0 is player
    public int Side { 
        get { return side; }
        set { side = value; }
    }
}
