using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cinemachine;

public class GameInitializer : MonoBehaviour
{
    [Header("Addressables")]
    [SerializeField] private string playerAddressableKey = "PlayerPrefab";

    [Header("Spawn")]
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera vcamTopDown;

    [Header("Systems (drag in inspector)")]
    [SerializeField] private ActivateButton activateButton;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private WaveSpawner waveSpawner;

    private CancellationTokenSource cancellationTokenSource;
    private GameObject playerInstance;

    private void Awake()
    {
        cancellationTokenSource = new CancellationTokenSource();
        _ = InitializeGameAsync(cancellationTokenSource.Token);
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    private async Task InitializeGameAsync(CancellationToken cancellationToken)
    {
        try
        {
            Debug.Log("[GameInitializer] Starting initialization...");

            var initHandle = Addressables.InitializeAsync();
            await initHandle.Task;

            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log("[GameInitializer] Initialization cancelled");
                return;
            }

            if (playerSpawnPoint == null)
            {
                Debug.LogError("[GameInitializer] Player spawn point not assigned!");
                return;
            }

            var playerHandle = Addressables.LoadAssetAsync<GameObject>(playerAddressableKey);
            await playerHandle.Task;

            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log("[GameInitializer] Initialization cancelled after loading player");
                Addressables.Release(playerHandle);
                return;
            }

            if (playerHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[GameInitializer] Failed to load player prefab: {playerHandle.OperationException}");
                return;
            }

            playerInstance = Instantiate(
                playerHandle.Result,
                playerSpawnPoint.position,
                Quaternion.identity
            );

            if (playerInstance == null)
            {
                Debug.LogError("[GameInitializer] Failed to instantiate player!");
                Addressables.Release(playerHandle);
                return;
            }

            Debug.Log("[GameInitializer] Player instantiated successfully");

            if (vcamTopDown != null)
            {
                vcamTopDown.Follow = playerInstance.transform;
            }
            else
            {
                Debug.LogWarning("[GameInitializer] Virtual camera not assigned");
            }

            InitializePlayerSystems();

            Addressables.Release(playerHandle);

            Debug.Log("[GameInitializer] Initialization completed successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameInitializer] Initialization failed with exception: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void InitializePlayerSystems()
    {
        if (playerInstance == null)
        {
            Debug.LogError("[GameInitializer] Cannot initialize systems - player instance is null");
            return;
        }

        var chipMagnet = playerInstance.GetComponent<ChipMagnet>();
        var handVisual = playerInstance.GetComponent<HandVisualManager>();
        var activationAnimator = playerInstance.GetComponent<ActivationAnimator>();
        var health = playerInstance.GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("[GameInitializer] Player missing Health component!");
        }

        if (activateButton != null)
        {
            if (chipMagnet != null && handVisual != null && activationAnimator != null)
            {
                activateButton.Init(chipMagnet, handVisual, activationAnimator);
                Debug.Log("[GameInitializer] Activate button initialized");
            }
            else
            {
                Debug.LogWarning("[GameInitializer] Cannot initialize activate button - missing player components");
            }
        }
        else
        {
            Debug.LogWarning("[GameInitializer] Activate button not assigned");
        }

        if (hudManager != null && health != null)
        {
            if (RunStateController.Instance != null)
            {
                hudManager.Init(health, RunStateController.Instance);
                Debug.Log("[GameInitializer] HUD manager initialized");
            }
            else
            {
                Debug.LogError("[GameInitializer] RunStateController instance not found!");
            }
        }
        else
        {
            if (hudManager == null)
                Debug.LogWarning("[GameInitializer] HUD manager not assigned");
            if (health == null)
                Debug.LogWarning("[GameInitializer] Player health component not found");
        }

        if (PlayerEvents.Instance != null)
        {
            PlayerEvents.Instance.NotifySpawned();
            Debug.Log("[GameInitializer] Player spawn event notified");
        }
        else
        {
            Debug.LogWarning("[GameInitializer] PlayerEvents instance not found!");
        }

        if (RunStateController.Instance != null)
        {
            RunStateController.Instance.StartNewRun();
            Debug.Log("[GameInitializer] Run started");
        }
        else
        {
            Debug.LogError("[GameInitializer] Cannot start run - RunStateController not found!");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(playerAddressableKey))
        {
            Debug.LogWarning("[GameInitializer] Player addressable key is empty!");
        }

        if (playerSpawnPoint == null)
        {
            Debug.LogWarning("[GameInitializer] Player spawn point not assigned!");
        }

        if (vcamTopDown == null)
        {
            Debug.LogWarning("[GameInitializer] Virtual camera not assigned!");
        }

        if (activateButton == null)
        {
            Debug.LogWarning("[GameInitializer] Activate button not assigned!");
        }

        if (hudManager == null)
        {
            Debug.LogWarning("[GameInitializer] HUD manager not assigned!");
        }
    }
#endif
}