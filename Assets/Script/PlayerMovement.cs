using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float stopDistance;
    public float gridSize;

    public LayerMask groundLayer;
    public LayerMask collectibleLayer;
    public float interactionRange;

    private Vector3 initialTargetPosition;
    private Vector3 currentSegmentTarget;
    private bool isMoving = false;
    private bool movingHorizontalSegment = false;

    private CollectibleItem targetCollectible = null;
    private Vector3 interactionPoint;
    private bool attemptingCollection = false;

    void Start()
    {
        transform.position = SnapToGrid(transform.position);
        initialTargetPosition = transform.position;
        currentSegmentTarget = transform.position;
    }

    void Update()
    {
        HandleInput();
        MovePlayer();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            targetCollectible = null;
            attemptingCollection = false;

            RaycastHit2D hitCollectible = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, collectibleLayer);

            if (hitCollectible.collider == null)
            {
                RaycastHit2D hitGround = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, groundLayer);

                if (hitGround.collider != null)
                {
                    initialTargetPosition = SnapToGrid(hitGround.point);
                    initialTargetPosition.z = transform.position.z;
                    StartMovementSegments();
                }
                else
                {
                    isMoving = false;
                }
            }
            else
            {
                targetCollectible = hitCollectible.collider.GetComponent<CollectibleItem>();
                if (targetCollectible != null)
                {
                    Vector3 snappedItemPos = SnapToGrid(targetCollectible.transform.position);
                    interactionPoint = GetClosestCardinalPoint(snappedItemPos, transform.position);

                    initialTargetPosition = SnapToGrid(interactionPoint);
                    initialTargetPosition.z = transform.position.z;

                    attemptingCollection = true;
                    StartMovementSegments();
                }
                else
                {
                    isMoving = false;
                }
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, position.z);
    }

    private Vector3 GetClosestCardinalPoint(Vector3 itemPos, Vector3 playerPos)
    {
        Vector3 snappedPlayerPos = SnapToGrid(playerPos);
        Vector3 directionToPlayer = (snappedPlayerPos - itemPos).normalized;

        Vector3 closestCardinalDir = Vector3.zero;
        float maxDot = -1f;

        Vector3[] cardinalDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };

        foreach (Vector3 dir in cardinalDirections)
        {
            float dot = Vector3.Dot(directionToPlayer, dir);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestCardinalDir = dir;
            }
        }
        return itemPos + closestCardinalDir * gridSize;
    }


    void StartMovementSegments()
    {
        initialTargetPosition = SnapToGrid(initialTargetPosition);
        transform.position = SnapToGrid(transform.position);

        isMoving = true;

        if (Mathf.Abs(initialTargetPosition.x - transform.position.x) > stopDistance)
        {
            currentSegmentTarget = new Vector3(initialTargetPosition.x, transform.position.y, transform.position.z);
            currentSegmentTarget = SnapToGrid(currentSegmentTarget);
            movingHorizontalSegment = true;
        }
        else if (Mathf.Abs(initialTargetPosition.y - transform.position.y) > stopDistance)
        {
            currentSegmentTarget = new Vector3(transform.position.x, initialTargetPosition.y, transform.position.z);
            currentSegmentTarget = SnapToGrid(currentSegmentTarget);
            movingHorizontalSegment = false;
        }
        else
        {
            isMoving = false;
            transform.position = initialTargetPosition;

            if (attemptingCollection)
            {
                CheckAndCollectItem();
            }
        }
    }

    void MovePlayer()
    {
        if (isMoving)
        {
            float distanceToSegmentTarget = Vector3.Distance(transform.position, currentSegmentTarget);

            if (distanceToSegmentTarget <= stopDistance)
            {
                transform.position = currentSegmentTarget;

                if (movingHorizontalSegment)
                {
                    if (Mathf.Abs(initialTargetPosition.y - transform.position.y) > stopDistance)
                    {
                        currentSegmentTarget = new Vector3(transform.position.x, initialTargetPosition.y, transform.position.z);
                        currentSegmentTarget = SnapToGrid(currentSegmentTarget);
                        movingHorizontalSegment = false;
                    }
                    else
                    {
                        isMoving = false;
                        if (attemptingCollection) CheckAndCollectItem();
                    }
                }
                else
                {
                    isMoving = false;
                    if (attemptingCollection) CheckAndCollectItem();
                }
            }

            if (isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentSegmentTarget, moveSpeed * Time.deltaTime);
            }
        }
    }

    void CheckAndCollectItem()
    {
        if (targetCollectible != null)
        {
            Vector3 snappedItemPos = SnapToGrid(targetCollectible.transform.position);
            float distanceToItem = Vector3.Distance(transform.position, snappedItemPos);

            if (Mathf.Abs(distanceToItem - gridSize) < stopDistance)
            {
                targetCollectible.Collect();
                targetCollectible = null;
                attemptingCollection = false;
            }
            else
            {
                targetCollectible = null;
                attemptingCollection = false;
            }
        }
    }
}