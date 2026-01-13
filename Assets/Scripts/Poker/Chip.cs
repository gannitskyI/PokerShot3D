using UnityEngine;

public class Chip : MonoBehaviour
{
    public enum Suit { Heart, Diamond, Club, Spade }
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public Suit suit;
    public Rank rank;

    public void SetRandomCard()
    {
        suit = (Suit)Random.Range(0, 4);
        rank = (Rank)Random.Range(2, 15); // 2–14 (Ace=14)
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Позже: меняем цвет/material по suit
        Debug.Log($"Чип: {rank} of {suit}");
    }
}