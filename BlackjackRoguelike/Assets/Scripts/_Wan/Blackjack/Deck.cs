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
            if (suit == CardSuit.None) continue;
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
            {
                if (rank == CardRank.None) continue;
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

    // 지정한 숫자와 일치하는 카드 한 장을 덱에서 뽑습니다.
    public Card DrawByRank(CardRank rank)
    {
        for (int _index = cards.Count - 1; _index >= 0; _index--)
        {
            if (cards[_index].Rank != rank) continue;

            Card _card = cards[_index];
            cards.RemoveAt(_index);
            return _card;
        }

        Reset();
        return DrawByRank(rank);
    }

    // 교체로 제외했던 카드를 덱에 되돌린 뒤 다시 섞습니다.
    public void ReturnCards(IEnumerable<Card> returnedCards)
    {
        if (returnedCards == null) return;
        cards.AddRange(returnedCards);
        Shuffle();
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
