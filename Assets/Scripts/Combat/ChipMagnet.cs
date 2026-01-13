using UnityEngine;
using System.Collections.Generic;

public class ChipMagnet : MonoBehaviour
{
    [Header("Магнит")]
    [SerializeField] private float magnetRange = 4f;       // радиус притяжения
    [SerializeField] private float magnetSpeed = 8f;       // скорость полёта чипа к игроку

    [Header("Покер-рука")]
    public List<Chip> hand = new List<Chip>(5);           // макс 5 чипов

    private void Update()
    {
        // Ищем все чипы в радиусе
        Collider[] chipsInRange = Physics.OverlapSphere(transform.position, magnetRange, LayerMask.GetMask("Chip"));

        foreach (var col in chipsInRange)
        {
            Chip chip = col.GetComponent<Chip>();
            if (chip != null && !hand.Contains(chip))
            {
                // Притягиваем чип к игроку
                chip.transform.position = Vector3.MoveTowards(
                    chip.transform.position,
                    transform.position + Vector3.up * 0.5f,  // чуть выше игрока
                    magnetSpeed * Time.deltaTime
                );

                // Если близко — собираем в руку
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
            // Discard старый (рандомно или первый)
            Chip old = hand[0];
            hand.RemoveAt(0);
            ChipPool.Instance.ReturnChip(old.gameObject);
            Debug.Log("[ChipMagnet] Рука полная — discard старого чипа");
        }

        hand.Add(chip);
        var result = PokerEvaluator.EvaluateHand(hand);
        Debug.Log($"[Poker] Рука: {result.description} (x{result.multiplier})");
        chip.gameObject.SetActive(false);  // пока прячем, потом можно визуализировать в UI

        Debug.Log($"[ChipMagnet] Собран чип: {chip.rank} of {chip.suit} | Рука: {hand.Count}/5");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}