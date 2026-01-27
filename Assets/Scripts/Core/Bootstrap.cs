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

            // Камера
            if (vcamTopDown != null)
                vcamTopDown.Follow = playerInstance.transform;

            // Передача ссылок
            var activateButton = FindObjectOfType<ActivateButton>();
            if (activateButton != null)
            {
                var chipMagnet = playerInstance.GetComponent<ChipMagnet>();
                var handVisual = playerInstance.GetComponent<HandVisualManager>();
                var activationAnimator = playerInstance.GetComponent<ActivationAnimator>();
                if (chipMagnet != null && handVisual != null && activationAnimator != null)
                    activateButton.Init(chipMagnet, handVisual, activationAnimator);
            }

            // Теперь стартуем run (игрок уже спавнен)
            RunStateController.Instance.StartNewRun();

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