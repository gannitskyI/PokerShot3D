using UnityEngine;
using System.Collections.Generic;

public class ChipPool : MonoBehaviour
{
    public static ChipPool Instance { get; private set; }

    [SerializeField] private GameObject chipPrefab;           
    [SerializeField] private int initialPoolSize = 100;      

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);   
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
    
    }

    public GameObject GetChip()
    {
        if (pool.Count == 0)
        {
  
            GameObject chip = Instantiate(chipPrefab);
            chip.SetActive(false);
            pool.Enqueue(chip);
          
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnChip(GameObject chip)
    {
        chip.SetActive(false);
        chip.transform.position = new Vector3(1000, 1000, 1000);   
        chip.transform.rotation = Quaternion.identity;
        pool.Enqueue(chip);
       
    }
}