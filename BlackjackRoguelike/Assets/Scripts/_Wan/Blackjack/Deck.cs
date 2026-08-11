using System;
using System.Collections.Generic;

// <summary>
// Deck 클래스: 카드 덱을 나타내는 클래스입니다. 이 클래스는 카드의 생성, 섞기, 뽑기 등의 기능을 제공합니다. 또한, 덱의 상태를 관리하며, 필요 시 덱을 초기화할 수 있습니다.
// </summary>
public sealed class Deck
{
    private readonly List<Card> cards = new();
    private readonly Random random;

    public int Count => cards.Count;

    public Deck(int? seed = null)
    {
        random = seed.HasValue ? new Random(seed.Value) : new Random();
        Reset();
    }

    public void Reset()
    {
        cards.Clear();
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
            {
                cards.Add(new Card(suit, rank));
            }
        }

        Shuffle();
    }

    public Card Draw()
    {
        if (cards.Count == 0)
        {
            Reset();
        }

        int lastIndex = cards.Count - 1;
        Card card = cards[lastIndex];
        cards.RemoveAt(lastIndex);
        return card;
    }

    private void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (cards[i], cards[swapIndex]) = (cards[swapIndex], cards[i]);
        }
    }
}