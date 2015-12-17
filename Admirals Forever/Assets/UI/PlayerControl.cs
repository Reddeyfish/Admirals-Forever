using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class PlayerControl : MonoBehaviour, IObserver<DoubleLeftMouseClickMessage>, IObserver<LeftMouseDragStartMessage>, IObserver<LeftMouseDraggingMessage>, IObserver<LeftMouseDragEndMessage>,
    IObserver<RightMouseDragStartMessage>, IObserver<RightMouseDraggingMessage>, IObserver<RightMouseDragEndMessage>,
    IObserver<ShipDestroyedMessage>
{
    [SerializeField]
    protected GameObject enemyShip;

    [SerializeField]
    protected float dragVsClickTolerance;

    [SerializeField]
    protected float targetingFudgeDistance;

    [SerializeField]
    protected float screenEdgeDistance;

    [SerializeField]
    protected float screenScrollSpeed;

    [SerializeField]
    protected float mouseScrollSensitivity;

    [SerializeField]
    protected float maximumZoom;

    [SerializeField]
    [AutoLink]
    protected MouseInput input;

    [SerializeField]
    [AutoLink(parentName=Tags.canvas, parentTag=Tags.canvas)]
    protected Transform canvas;

    [SerializeField]
    [AutoLink(parentName = "Reticle", parentTag = Tags.canvas)]
    protected RectTransform reticle;

    [SerializeField]
    [AutoLink(parentName = "SelectionBox", parentTag = Tags.canvas)]
    protected RectTransform selectionBox;

    HashSet<Navigation> selectedShips = new HashSet<Navigation>();
    float groupMaxSpeed = float.MaxValue;
    float groupAccel = float.MaxValue;
    void AddShip(Navigation ship)
    {
        selectedShips.Add(ship);
        ship.Selection = Navigation.SelectionMode.SELECTED;
        ship.GetComponent<Ship>().Subscribe<ShipDestroyedMessage>(this);
        if (ship.MaxSpeed < groupMaxSpeed) 
            groupMaxSpeed = ship.MaxSpeed; 
        if (ship.Accel < groupAccel) 
            groupAccel = ship.Accel;
    }
    void ClearShips()
    {
        foreach (Navigation ship in selectedShips)
            _unselectShip(ship);

        selectedShips.Clear();
        SetDefaultSpeedAndAccel();
    }
    void RemoveShip(Navigation ship)
    {
        selectedShips.Remove(ship);
        _unselectShip(ship);
        if (selectedShips.Count == 0)
            SetDefaultSpeedAndAccel();
        else
        {
            groupMaxSpeed = selectedShips.Min((Navigation nav) => nav.MaxSpeed);
            groupAccel = selectedShips.Min((Navigation nav) => nav.Accel);
        }
    }

    void _unselectShip(Navigation ship)
    {
        ship.Selection = Navigation.SelectionMode.UNSELECTED;
        ship.GetComponent<Ship>().Unsubscribe<ShipDestroyedMessage>(this);
    }
    void ToggleShip(Navigation ship)
    {
        if (selectedShips.Contains(ship))
            RemoveShip(ship);
        else
            AddShip(ship);
    }
    void SetDefaultSpeedAndAccel() { groupMaxSpeed = float.MaxValue; groupAccel = float.MaxValue; }

    HashSet<Navigation> highlightedShips = new HashSet<Navigation>();

    void UpdateHighlightedShips(HashSet<Navigation> newHighlightedShips)
    {
        foreach (Navigation ship in highlightedShips)
        {
            if (!newHighlightedShips.Contains(ship))
            {
                if (selectedShips.Contains(ship))
                {
                    ship.Selection = Navigation.SelectionMode.SELECTED;
                }
                else
                {
                    ship.Selection = Navigation.SelectionMode.UNSELECTED;
                }
            }
        }

        foreach (Navigation ship in newHighlightedShips)
        {
            if (!highlightedShips.Contains(ship))
            {
                ship.Selection = Navigation.SelectionMode.HIGHLIGHTED;
            }
        }
        highlightedShips = newHighlightedShips;
    }

    int screenWidth;
    int screenHeight;

    LineRenderer lineRenderer;

    LayerMask shipLayerMask;

    Transform mainCameraTransform;
    Camera mainCamera;

	// Use this for initialization
	void Awake () {
        input.Subscribe<DoubleLeftMouseClickMessage>(this);
        input.Subscribe<LeftMouseDragStartMessage>(this);
        input.Subscribe<LeftMouseDraggingMessage>(this);
        input.Subscribe<LeftMouseDragEndMessage>(this);
        input.Subscribe<RightMouseDragStartMessage>(this);
        input.Subscribe<RightMouseDraggingMessage>(this);
        input.Subscribe<RightMouseDragEndMessage>(this);
        lineRenderer = GetComponent<LineRenderer>();

        screenWidth = Screen.width;
        screenHeight = Screen.height;

        shipLayerMask = LayerMask.GetMask(Tags.Layers.ships);
	}

    void Start()
    {
        mainCamera = Camera.main;
        Assert.IsTrue(mainCamera.orthographic);
        mainCameraTransform = mainCamera.transform;
        foreach (Navigation ship in FindObjectsOfType<Navigation>())
        {
            AddShip(ship);
        }
    }
	
	// Update is called once per frame
	void Update () {
        mainCamera.orthographicSize = Mathf.Max(maximumZoom, mainCamera.orthographicSize + mouseScrollSensitivity * Input.GetAxis(Tags.Axis.mouseScrollWheel));

        if (Input.mousePosition.x <= screenEdgeDistance || Input.GetKey(KeyCode.LeftArrow))
            mainCameraTransform.position += (Vector3)(screenScrollSpeed * Mathf.Sqrt(mainCamera.orthographicSize) * Time.deltaTime * Vector2.left);
        if (Input.mousePosition.y <= screenEdgeDistance || Input.GetKey(KeyCode.DownArrow))
            mainCameraTransform.position += (Vector3)(screenScrollSpeed * Mathf.Sqrt(mainCamera.orthographicSize) * Time.deltaTime * Vector2.down);

        if (Input.mousePosition.x >= screenWidth - screenEdgeDistance || Input.GetKey(KeyCode.RightArrow))
            mainCameraTransform.position += (Vector3)(screenScrollSpeed * Mathf.Sqrt(mainCamera.orthographicSize) * Time.deltaTime * Vector2.right);
        if (Input.mousePosition.y >= screenHeight - screenEdgeDistance || Input.GetKey(KeyCode.UpArrow))
            mainCameraTransform.position += (Vector3)(screenScrollSpeed * Mathf.Sqrt(mainCamera.orthographicSize) * Time.deltaTime * Vector2.up);

        UpdateHighlightedShips(hitFriendlies(Input.mousePosition.toWorldPoint()));
	}

    public void Notify(ShipDestroyedMessage m)
    {
        RemoveShip(m.destroyedShip.GetComponent<Navigation>());
    }

    public void Notify(DoubleLeftMouseClickMessage m)
    {
        Ship spawnedShip = (Instantiate(enemyShip, m.worldPoint, RandomLib.Random2DRotation()) as GameObject).GetComponent<Ship>();
        spawnedShip.Side = 1;
    }

    public void Notify(LeftMouseDragStartMessage m)
    {

    }

    public void Notify(LeftMouseDraggingMessage m)
    {
        if ((m.current - m.start).magnitude > dragVsClickTolerance)
        {
            selectionBox.gameObject.SetActive(true);
            Vector2 center = (m.current + m.start) / 2;
            Vector2 magnitude = new Vector2(Mathf.Abs(m.current.x - center.x), Mathf.Abs(m.current.y - center.y));
            selectionBox.position = center;
            selectionBox.sizeDelta = magnitude;
        }
        else
        {
            selectionBox.gameObject.SetActive(false);
        }
    }

    public void Notify(LeftMouseDragEndMessage m)
    {
        selectionBox.gameObject.SetActive(false);

        HashSet<Navigation> hits;
        if ((m.end - m.start).magnitude > dragVsClickTolerance)
            hits = hitFriendlies(m.startWorldPoint, m.endWorldPoint);
        else
            hits = hitFriendlies(m.endWorldPoint);

        if (MultipleSelect())
        {
            foreach (Navigation ship in hits)
            {
                ToggleShip(ship);
            }
        }
        else
        {
            ClearShips();
            foreach(Navigation ship in hits)
                AddShip(ship);
        }
        
    }

    public void Notify(RightMouseDragStartMessage m)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, m.worldPoint);
        lineRenderer.SetPosition(1, m.worldPoint);
    }

    public void Notify(RightMouseDraggingMessage m)
    {
        if (hitEnemy(m.currentWorldPoint) != null)
        {
            reticle.gameObject.SetActive(true);
            reticle.position = m.current;

            lineRenderer.SetPosition(1, m.startWorldPoint);
        }
        else if ((m.current - m.start).magnitude > dragVsClickTolerance)
        {
            //if it's large enough to count as a drag
            lineRenderer.SetPosition(1, m.currentWorldPoint);

            reticle.gameObject.SetActive(false);
        }
        else
        {
            lineRenderer.SetPosition(1, m.startWorldPoint);
            reticle.gameObject.SetActive(false);
        }
    }
    HashSet<Navigation> hitFriendlies(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, targetingFudgeDistance, shipLayerMask);
        return getFriendlies(hits);
    }
    HashSet<Navigation> hitFriendlies(Vector2 startPosition, Vector2 endPosition)
    {
        Collider2D[] hits = Physics2D.OverlapAreaAll(startPosition, endPosition, shipLayerMask);
        return getFriendlies(hits);
    }
    HashSet<Navigation> getFriendlies(Collider2D[] hits)
    {
        HashSet<Navigation> results = new HashSet<Navigation>();
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(Tags.ship))
            {
                Navigation ship = hit.GetComponentInParent<Navigation>();
                if (ship != null)
                    results.Add(ship);
            }
        }
        return results;
    }

    Ship hitEnemy(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, targetingFudgeDistance, shipLayerMask);
        if (hits.Length != 0)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag(Tags.ship))
                {
                    Ship ship = hit.GetComponentInParent<Ship>();
                    if (ship != null && ship.Side != 0) //enemy ship
                        return ship;
                }
            }
        }
        return null;
    }

    public void Notify(RightMouseDragEndMessage m)
    {
        lineRenderer.enabled = false;
        reticle.gameObject.SetActive(false);

        Ship target = hitEnemy(m.endWorldPoint);
        if (target != null)
        {
            checkClearPreviousAttackOrders(selectedShips);
            foreach (Navigation selectedShip in selectedShips)
                selectedShip.addAttackOrder(new AttackOrder(target));
        }
        else if (selectedShips.Count != 0)
        {
            //movement orders
            Vector2 centerPoint = Vector2.zero;
            foreach (Navigation selectedShip in selectedShips)
                centerPoint += (Vector2)(selectedShip.transform.position);
            centerPoint /= selectedShips.Count;

            if ((m.end - m.start).magnitude > dragVsClickTolerance)
            {
                //then treat it as a drag
                checkClearPreviousMovementOrders(selectedShips);
                Vector2 direction = m.end.toWorldPoint() - m.start.toWorldPoint();
                foreach (Navigation selectedShip in selectedShips)
                    selectedShip.addMovementOrder(new MoveFaceOrder(formationPoint(selectedShip, centerPoint, m.startWorldPoint), groupMaxSpeed, groupAccel, direction));
            }
            else
            {
                //then treat it as a click
                checkClearPreviousMovementOrders(selectedShips);
                foreach (Navigation selectedShip in selectedShips)
                    selectedShip.addMovementOrder(new MovementOrder(formationPoint(selectedShip, centerPoint, m.endWorldPoint), groupMaxSpeed, groupAccel));
            }
        }
    }

    Vector2 formationPoint(Navigation selectedShip, Vector2 centerPoint, Vector2 targetPoint)
    {
        return targetPoint + ((Vector2)(selectedShip.transform.position) - centerPoint);
    }

    void checkClearPreviousAttackOrders(HashSet<Navigation> selectedShips)
    {
        if (!MultipleSelect())
        {
            foreach(Navigation selectedShip in selectedShips)
                selectedShip.clearAttackOrders();
        }
    }

    void checkClearPreviousMovementOrders(HashSet<Navigation> selectedShips)
    {
        if (!MultipleSelect())
        {
            foreach (Navigation selectedShip in selectedShips)
                selectedShip.clearMovementOrders();
        }
    }

    bool MultipleSelect()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}
