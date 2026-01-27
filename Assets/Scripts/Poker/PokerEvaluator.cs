using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PokerEvaluator
{
    public enum HandType
    {
        HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush,
        FullHouse, FourOfAKind, StraightFlush, RoyalFlush
    }

    public struct HandResult
    {
        public HandType type;
        public int multiplier;     
        public string description;
    }

    public static HandResult EvaluateHand(List<Chip> hand)
    {
        if (hand.Count != 5)
            return new HandResult { type = HandType.HighCard, multiplier = 1, description = "HighCard" };
 
        var sorted = hand.OrderByDescending(c => (int)c.rank).ToList();

        bool isFlush = sorted.All(c => c.suit == sorted[0].suit);
        bool isStraight = IsStraight(sorted);
        bool isRoyal = isStraight && sorted[0].rank == Chip.Rank.Ace && sorted[4].rank == Chip.Rank.Ten;

        var rankCounts = sorted.GroupBy(c => c.rank)
                               .ToDictionary(g => g.Key, g => g.Count());

        int maxCount = rankCounts.Values.Max();

        if (isRoyal && isFlush)
            return new HandResult { type = HandType.RoyalFlush, multiplier = 10, description = "Royal Flush" };

        if (isStraight && isFlush)
            return new HandResult { type = HandType.StraightFlush, multiplier = 8, description = "Straight Flush" };

        if (maxCount == 4)
            return new HandResult { type = HandType.FourOfAKind, multiplier = 7, description = "Four of a Kind" };

        if (rankCounts.Values.Contains(3) && rankCounts.Values.Contains(2))
            return new HandResult { type = HandType.FullHouse, multiplier = 6, description = "Full House" };

        if (isFlush)
            return new HandResult { type = HandType.Flush, multiplier = 5, description = "Flush" };

        if (isStraight)
            return new HandResult { type = HandType.Straight, multiplier = 4, description = "Straight" };

        if (maxCount == 3)
            return new HandResult { type = HandType.ThreeOfAKind, multiplier = 3, description = "Three of a Kind" };

        if (rankCounts.Values.Count(v => v == 2) == 2)
            return new HandResult { type = HandType.TwoPair, multiplier = 2, description = "Two Pair" };

        if (maxCount == 2)
            return new HandResult { type = HandType.Pair, multiplier = 2, description = "Pair" };

        return new HandResult { type = HandType.HighCard, multiplier = 1, description = "High Card" };
    }

    private static bool IsStraight(List<Chip> sorted)
    {
        for (int i = 1; i < 5; i++)
        {
            if ((int)sorted[i].rank != (int)sorted[i - 1].rank - 1)
                return false;
        }
        return true;
    }
}