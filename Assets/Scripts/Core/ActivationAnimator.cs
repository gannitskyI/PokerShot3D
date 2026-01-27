using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.VFX;

[RequireComponent(typeof(ChipMagnet))]
public class ActivationAnimator : MonoBehaviour
{
    [Header("Card Spread")]
    [SerializeField] private float explodeRadius = 5f;
    [SerializeField] private float explodeDuration = 0.8f;

    [Header("Shake")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("VFX")]
    [SerializeField] private VisualEffect activationBurst;

    private ChipMagnet chipMagnet;
    private HandVisualManager handVisual;

    private void Awake()
    {
        chipMagnet = GetComponent<ChipMagnet>();
        handVisual = GetComponent<HandVisualManager>();

        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>() ?? gameObject.AddComponent<CinemachineImpulseSource>();
    }

    public void PlayActivation(PokerEvaluator.HandResult result)
    {
        if (handVisual == null) return;

        if (activationBurst != null)
        {
            activationBurst.Stop();
            activationBurst.SetVector4("StartColor", GetColorByHand(result.type));
            activationBurst.Play();
            Debug.Log("[ActivationAnimator] VFX burst: Stop + Play + color set");
        }


        if (impulseSource != null)
        {
            float shakeForce = result.multiplier * 0.5f;
            impulseSource.GenerateImpulse(new Vector3(shakeForce, shakeForce * 0.5f, 0));
        }


        var cards = handVisual.GetCurrentCards();
        foreach (var card in cards)
        {
            if (card == null) continue;

            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);


            card.transform.DOMove(card.transform.position + randomDir * explodeRadius, explodeDuration)
                .SetEase(Ease.OutExpo)
                .OnComplete(() => SafeDestroy(card));

            card.transform.DOScale(Vector3.zero, explodeDuration * 0.6f)
                .SetDelay(explodeDuration * 0.4f);

            card.transform.DORotate(Random.insideUnitSphere * 360f, explodeDuration, RotateMode.FastBeyond360);
        }

        if (cards.Length == 0)
            handVisual.ClearHandVisual();
    }

    private void SafeDestroy(GameObject obj)
    {
        if (obj != null)
            Destroy(obj);
    }

    private Color GetColorByHand(PokerEvaluator.HandType type)
    {
        return type switch
        {
            PokerEvaluator.HandType.RoyalFlush => new Color(1f, 0.2f, 0.2f),
            PokerEvaluator.HandType.Flush => new Color(0.2f, 0.2f, 1f),
            PokerEvaluator.HandType.FullHouse => new Color(0.8f, 0f, 0.8f),
            _ => Color.white
        };
    }
}