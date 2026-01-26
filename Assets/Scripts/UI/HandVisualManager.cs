using UnityEngine;
using System.Collections.Generic;

public class HandVisualManager : MonoBehaviour
{
    [Header("Слоты")]
    [SerializeField] private Transform[] cardSlots = new Transform[5];   

    [Header("Визуал")]
    [SerializeField] private GameObject cardVisualPrefab;  // CardVisual.prefab
    [SerializeField] private float flySpeed = 10f;         // скорость влёта

    private List<GameObject> currentCards = new List<GameObject>();
    private ChipMagnet chipMagnet;

    private void Awake()
    {
        chipMagnet = GetComponent<ChipMagnet>();
        if (chipMagnet == null) Debug.LogError("[HandVisual] ChipMagnet не найден!");
    }
    public void Init()
    {
        Debug.Log("[HandVisualManager] Инициализирован");
    }
    public GameObject[] GetCurrentCards()
    {
        return currentCards.ToArray();
    }
    public void AddCardVisual(Chip.CardData cardData, Vector3 fromPosition)
    {
        if (currentCards.Count >= 5) return;

        int slotIndex = currentCards.Count;
        Transform slot = cardSlots[slotIndex];

        GameObject visualCard = Instantiate(cardVisualPrefab, fromPosition, Quaternion.identity, slot);
        visualCard.GetComponent<Chip>().SetCard(cardData.suit, cardData.rank);  // масть/ранг

        // Анимация влёта
        StartCoroutine(FlyInAnimation(visualCard.transform, slot.position));

        currentCards.Add(visualCard);
    }

    private System.Collections.IEnumerator FlyInAnimation(Transform card, Vector3 targetPos)
    {
        while (Vector3.Distance(card.position, targetPos) > 0.1f)
        {
            card.position = Vector3.MoveTowards(card.position, targetPos, flySpeed * Time.deltaTime);
            yield return null;
        }

        card.position = targetPos;
    }

    public void ClearHandVisual()
    {
        foreach (var card in currentCards)
        {
            Destroy(card);
        }
        currentCards.Clear();
    }
}