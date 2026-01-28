using UnityEngine;
using System.Collections.Generic;

public class ChipMagnet : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float magnetRange = 4f;
    [SerializeField] private float magnetSpeed = 8f;

    [Header("Poker Hand")]
    public List<Chip> hand = new List<Chip>(5);

    [Header("Events")]
    [SerializeField] private PlayerEvents playerEvents;
    [SerializeField] private HandVisualManager handVisualManager;

    private void Awake()
    {
        if (playerEvents == null)
            playerEvents = GetComponent<PlayerEvents>();
        if (playerEvents == null)
            Debug.LogError("[ChipMagnet] PlayerEvents not found!");
    }

    private void Update()
    {
        Collider[] chipsInRange = Physics.OverlapSphere(transform.position, magnetRange, LayerMask.GetMask("Chip"));
        foreach (var col in chipsInRange)
        {
            Chip chip = col.GetComponent<Chip>();
            if (chip != null && !hand.Contains(chip))
            {
                chip.transform.position = Vector3.MoveTowards(
                    chip.transform.position,
                    transform.position,
                    magnetSpeed * Time.deltaTime
                );
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
        }
        hand.Add(chip);
        if (handVisualManager != null)
        {
            handVisualManager.AddCardVisual(new Chip.CardData { suit = chip.suit, rank = chip.rank }, chip.transform.position);
        }
        ChipPool.Instance.ReturnChip(chip.gameObject);
        if (playerEvents != null)
            playerEvents.OnHandChanged.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}