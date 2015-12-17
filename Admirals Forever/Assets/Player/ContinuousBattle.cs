using UnityEngine;
using System.Collections;

public class ContinuousBattle : MonoBehaviour, IObserver<ShipDestroyedMessage> {

    [SerializeField]
    protected GameObject shipPrefab;

    [SerializeField]
    protected int numShips;

    [SerializeField]
    protected float spawningRange;

    [SerializeField]
    protected float spawningClearance;

    [SerializeField]
    protected int side;

    LayerMask shipLayerMask;


	// Use this for initialization
	void Start () {
        shipLayerMask = shipLayerMask = LayerMask.GetMask(Tags.Layers.ships);
        for (int i = 0; i < numShips; i++)
            SpawnShip();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Notify(ShipDestroyedMessage m)
    {
        SpawnShip();
        m.destroyedShip.Unsubscribe<ShipDestroyedMessage>(this);
    }

    void SpawnShip()
    {
        Vector2 spawnPoint = Vector2.zero;
        Collider2D hit = null; //assigned just to get the compiler to shut up
        for (int i = 0; i < 10; i++)
        {
            spawnPoint = spawningRange * Random.insideUnitCircle;
            hit = Physics2D.OverlapCircle(spawnPoint, spawningClearance, shipLayerMask);
            if (hit == null)
                break;
        }

        if (hit != null)
            Debug.Log("No Valid Spawning Places found");

        Ship spawnedShip = (Instantiate(shipPrefab, spawnPoint, RandomLib.Random2DRotation()) as GameObject).GetComponent<Ship>();
        spawnedShip.Side = side;
        spawnedShip.Subscribe<ShipDestroyedMessage>(this);
        Debug.Log("Spawned New Ship!");
    }
}
