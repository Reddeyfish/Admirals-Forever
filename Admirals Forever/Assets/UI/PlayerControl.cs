using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class PlayerControl : MonoBehaviour, IObserver<DoubleLeftMouseClickMessage>, IObserver<LeftMouseDragStartMessage>, IObserver<LeftMouseDraggingMessage>, IObserver<LeftMouseDragEndMessage>,
    IObserver<RightMouseDragStartMessage>, IObserver<RightMouseDraggingMessage>, IObserver<RightMouseDragEndMessage>
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

    public Navigation selectedShip;

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

    }

    public void Notify(LeftMouseDragEndMessage m)
    {

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
            checkClearPreviousAttackOrders(selectedShip);
            selectedShip.addAttackOrder(new AttackOrder(target));
        }
        else if ((m.end - m.start).magnitude > dragVsClickTolerance)
        {
            //then treat it as a drag
            checkClearPreviousMovementOrders(selectedShip);
            selectedShip.addMovementOrder(new MoveFaceOrder(m.start.toWorldPoint(), selectedShip.MaxSpeed, selectedShip.Accel, m.end.toWorldPoint() - m.start.toWorldPoint()));
        }
        else
        {
            //then treat it as a click
            checkClearPreviousMovementOrders(selectedShip);
            selectedShip.addMovementOrder(new MovementOrder(m.end.toWorldPoint(), selectedShip.MaxSpeed, selectedShip.Accel));
        }
    }

    void checkClearPreviousAttackOrders(Navigation selectedShip)
    {
        if (!MultipleSelect())
            selectedShip.clearAttackOrders();
    }

    void checkClearPreviousMovementOrders(Navigation selectedShip)
    {
        if (!MultipleSelect())
            selectedShip.clearMovementOrders();
    }

    bool MultipleSelect()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}
