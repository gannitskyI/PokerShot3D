using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TapToMove : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerConfig config;

    private PlayerInputActions inputActions;
    private Camera mainCamera;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool hasMoveTarget;
    private bool isFollowing;  // Hold-drag mode
    private Vector2 pressStartScreenPos;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        SetupRigidbody();

        if (config == null) Debug.LogWarning("[TapToMove] PlayerConfig не назначен!");
    }

    private void SetupRigidbody()
    {
        rb.freezeRotation = true;
        rb.useGravity = false;
        rb.linearDamping = config.drag;
    }

    private void OnEnable()
    {
        inputActions.Player.MovePress.started += OnPressStarted;
        inputActions.Player.MovePress.canceled += OnPressCanceled;
        inputActions.Player.Move.performed += OnPositionUpdate;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.MovePress.started -= OnPressStarted;
        inputActions.Player.MovePress.canceled -= OnPressCanceled;
        inputActions.Player.Move.performed -= OnPositionUpdate;
        inputActions.Player.Disable();
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        pressStartScreenPos = inputActions.Player.Move.ReadValue<Vector2>();
        isFollowing = false;
        hasMoveTarget = false;  // Нет движения до tap/drag
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        if (isFollowing)
        { 
            return;
        }

        // Чистый тап: установить initial цель (если не self)
        SetMoveTarget(pressStartScreenPos);
        if (!hasMoveTarget)
        {
            // Self-tap: immediate stop
            rb.linearVelocity = Vector3.zero;
           
        }
    }

    private void OnPositionUpdate(InputAction.CallbackContext context)
    {
        if (!inputActions.Player.MovePress.IsPressed()) return;

        Vector2 currentScreenPos = context.ReadValue<Vector2>();
        float dragDistance = Vector2.Distance(currentScreenPos, pressStartScreenPos);

        if (dragDistance > config.dragThresholdPx)
        {
            isFollowing = true;
            SetMoveTarget(currentScreenPos);  // ТОЛЬКО здесь для follow!
        }
        // Hold on place: ничего (0 jitter)
    }

    private void SetMoveTarget(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~LayerMask.GetMask("Player")))
        {
            Vector3 worldPos = hit.point;
            worldPos.y = transform.position.y;

            if (Vector3.Distance(worldPos, transform.position) < config.minTapDistance)
            {
               
                return;
            }

            targetPosition = worldPos;
            hasMoveTarget = true;
           
        }
    }

    private void FixedUpdate()
    {
        if (!hasMoveTarget) return;

        float speed = isFollowing ? config.moveSpeed * config.followSpeedMultiplier : config.moveSpeed;
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);

        // HARDC STOP: Всегда при близко (anti-jitter для всех modes)
        if (Vector3.Distance(transform.position, targetPosition) < config.stopDistance)
        {
            rb.linearVelocity = Vector3.zero;
            hasMoveTarget = false; 
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hasMoveTarget)
        {
            Gizmos.color = isFollowing ? Color.yellow : Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, config.stopDistance * 2);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}