using UnityEngine;
using System.Collections.Generic;

public class ChipPool : MonoBehaviour
{
    public static ChipPool Instance { get; private set; }

    [SerializeField] private GameObject chipPrefab;           // Перетащи сюда префаб чипа (Cube с Chip.cs)
    [SerializeField] private int initialPoolSize = 100;       // На старте создаём 100 чипов

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  // Чтобы пул жил между сценами (если добавим меню)

        PrewarmPool();
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject chip = Instantiate(chipPrefab);
            chip.SetActive(false);
            pool.Enqueue(chip);
        }
        Debug.Log($"[ChipPool] Создано {initialPoolSize} чипов в пуле");
    }

    public GameObject GetChip()
    {
        if (pool.Count == 0)
        {
            // Авто-расширение пула
            GameObject chip = Instantiate(chipPrefab);
            chip.SetActive(false);
            pool.Enqueue(chip);
            Debug.LogWarning("[ChipPool] Пул пуст — добавлен новый чип");
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnChip(GameObject chip)
    {
        chip.SetActive(false);
        chip.transform.position = new Vector3(1000, 1000, 1000);  // off-screen, чтобы не мешать
        chip.transform.rotation = Quaternion.identity;
        pool.Enqueue(chip);
        Debug.Log($"[ChipPool] Чип {chip.name} возвращён в пул");
    }
}