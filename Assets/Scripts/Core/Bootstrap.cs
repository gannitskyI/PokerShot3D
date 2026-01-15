using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Unity.Cinemachine;


public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string playerAddressableKey = "PlayerPrefab";  // Ключ Addressable
    [SerializeField] private PlayerConfig playerConfig;                     // SO
    [SerializeField] private Transform playerSpawnPoint;                    // Пустой GO в центре арены
    [SerializeField] private CinemachineCamera vcamTopDown;  // перетащи в инспекторе

    private void Start()
    {
        var initOp = Addressables.InitializeAsync();
        initOp.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("[Bootstrap] Addressables успешно инициализированы");
                RunStateController.Instance?.StartNewRun();
                SpawnPlayerAsync().ContinueWith(_ => Debug.Log("[Bootstrap] Инициализация завершена"));
            }
            else
            {
                Debug.LogError("[Bootstrap] Ошибка инициализации Addressables: " + handle.OperationException?.Message);
            }
        };
    }

    private async Task SpawnPlayerAsync()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(playerAddressableKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject playerPrefab = handle.Result;
            GameObject playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);

            // Камера следует за игроком
            if (vcamTopDown != null)
            {
                vcamTopDown.Follow = playerInstance.transform;
            }

            // Передача ChipMagnet в ActivateButton (один раз после спавна)
            var activateButton = FindObjectOfType<ActivateButton>();
            if (activateButton != null)
            {
                var chipMagnet = playerInstance.GetComponent<ChipMagnet>();
                var handVisual = playerInstance.GetComponent<HandVisualManager>();
                var activationAnimator = playerInstance.GetComponent<ActivationAnimator>();

                if (chipMagnet != null && handVisual != null && activationAnimator != null)
                {
                    activateButton.Init(chipMagnet, handVisual, activationAnimator);
                    Debug.Log("[Bootstrap] Всё передано в ActivateButton");
                }
                else
                {
                    Debug.LogError("[Bootstrap] Не все компоненты на игроке!");
                }
            }
            var comboEffects = playerInstance.GetComponent<ComboEffects>();

            Debug.Log("[Bootstrap] Инициализация завершена");
        }
        else
        {
            Debug.LogError($"[Bootstrap] Не удалось загрузить {playerAddressableKey}");
        }
    }
 

    private void OnDestroy()
    {
        // Cleanup Addressables (хорошая практика)
        Addressables.ReleaseInstance(gameObject);
    }
}