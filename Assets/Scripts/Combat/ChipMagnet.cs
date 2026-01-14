using UnityEngine;
using System.Collections.Generic;

public class ChipMagnet : MonoBehaviour
{
    [Header("Магнит")]
    [SerializeField] private float magnetRange = 4f; // радиус притяжения
    [SerializeField] private float magnetSpeed = 8f; // скорость полёта

    [Header("Покер-рука")]
    public List<Chip> hand = new List<Chip>(5); // макс 5 чипов

    [Header("События")]
    [SerializeField] private PlayerEvents playerEvents; // перетащи в инспекторе

    [SerializeField] private HandVisualManager handVisualManager;
    private void Awake()
    {
        if (playerEvents == null)
            playerEvents = GetComponent<PlayerEvents>();
        if (playerEvents == null)
            Debug.LogError("[ChipMagnet] PlayerEvents не найден!");
    }

    private void Update()
    {
        Collider[] chipsInRange = Physics.OverlapSphere(transform.position, magnetRange, LayerMask.GetMask("Chip"));

        foreach (var col in chipsInRange)
        {
            Chip chip = col.GetComponent<Chip>();
            if (chip != null && !hand.Contains(chip))
            {
                // Фикс: летит в центр, без + Y
                chip.transform.position = Vector3.MoveTowards(
                    chip.transform.position,
                    transform.position,  // центр игрока
                    magnetSpeed * Time.deltaTime
                );

                // Фикс: проверка расстояния по центру
                if (Vector3.Distance(chip.transform.position, transform.position) < 0.5f)
                {
                    CollectChip(chip);
                }
            }
        }
    }

    private void CollectChip(Chip chip)
    {
        if (hand.Count >= 5)
        {
            Chip old = hand[0];
            hand.RemoveAt(0);
            ChipPool.Instance.ReturnChip(old.gameObject);
            Debug.Log("[ChipMagnet] Рука полная — discard старого");
        }

        hand.Add(chip); 
        if (handVisualManager != null)
        {
            handVisualManager.AddCardVisual(new Chip.CardData { suit = chip.suit, rank = chip.rank }, chip.transform.position);
        }
        ChipPool.Instance.ReturnChip(chip.gameObject); // возвращаем в пул

        if (playerEvents != null)
            playerEvents.OnHandChanged.Invoke();

        var result = PokerEvaluator.EvaluateHand(hand);
        Debug.Log($"[Poker] Рука: {result.description} (x{result.multiplier})");
        Debug.Log($"[ChipMagnet] Собран: {chip.rank} of {chip.suit} | Рука: {hand.Count}/5");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}