using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents : MonoBehaviour
{
    public static PlayerEvents Instance { get; private set; }

    public UnityEvent OnPlayerSpawned = new UnityEvent();
    public UnityEvent OnHandChanged = new UnityEvent();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // ”брали DontDestroyOnLoad Ч не нужен, игрок спавнитс€ каждый run
    }

    public void NotifySpawned()
    {
        OnPlayerSpawned.Invoke();
        Debug.Log("[PlayerEvents] —павн уведомление отправлено");
    }
}