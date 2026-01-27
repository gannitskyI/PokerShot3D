using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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

    private async void Awake()
    {
        await InitializeGameAsync();
    }

    private async Task InitializeGameAsync()
    {
        var initHandle = Addressables.InitializeAsync();
        await initHandle.Task;

        var playerHandle = Addressables.LoadAssetAsync<GameObject>(playerAddressableKey);
        await playerHandle.Task;

        if (playerHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject playerInstance = Instantiate(playerHandle.Result, playerSpawnPoint.position, Quaternion.identity);

            if (vcamTopDown != null)
                vcamTopDown.Follow = playerInstance.transform;
             
            var chipMagnet = playerInstance.GetComponent<ChipMagnet>();
            var handVisual = playerInstance.GetComponent<HandVisualManager>();
            var activationAnimator = playerInstance.GetComponent<ActivationAnimator>();
            var health = playerInstance.GetComponent<Health>();

            if (activateButton != null && chipMagnet != null && handVisual != null && activationAnimator != null)
                activateButton.Init(chipMagnet, handVisual, activationAnimator);

            if (hudManager != null && health != null)
                hudManager.Init(health, state: RunStateController.Instance);

            RunStateController.Instance.StartNewRun();
             
        }
        else
        {
            Debug.LogError("[GameInitializer] error");
        }
    }
}