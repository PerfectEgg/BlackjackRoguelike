using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>한 참가자가 가진 카드와 에이스 규칙이 적용된 현재 점수를 관리합니다.</summary>
public sealed class BlackjackHand
{
    private readonly List<Card> _cards = new();
    private readonly int _targetScore;
    private int _openingHighAceIndex = -1;

    // 현재 패입니다.
    public IReadOnlyList<Card> Cards => _cards;
    // 현재 점수입니다.
    public int Score { get; private set; }
    // 소프트 핸드 여부입니다.
    public bool IsSoft { get; private set; }
    // 패에 에이스가 한 장 이상 있는지 나타냅니다.
    public bool HasAce => _cards.Any(card => card.Rank == CardRank.Ace);
    // 현재 패에 11점으로 고정된 초기 에이스가 있는지 나타냅니다.
    public bool HasOpeningHighAce => _openingHighAceIndex >= 0;
    // 현재 패에서 11점으로 처리하는 초기 에이스의 카드 인덱스입니다. 없으면 -1입니다.
    public int OpeningHighAceIndex => _openingHighAceIndex;
    // 블랙잭 여부입니다.
    public bool IsBlackjack => _cards.Count == 2 && Score == 21;

    // 마지막 카드가 삭제된 뒤 삭제 카드와 갱신된 점수를 전달합니다.
    public event Action<Card, int> LastCardRemoved;

    // 에이스 계산에 사용할 목표 점수를 지정합니다.
    public BlackjackHand(int targetScore = 21)
    {
        _targetScore = targetScore;
    }

    // 카드를 추가하고, 초기 패의 첫 에이스만 11점으로 고정합니다.
    public void Add(Card card, bool isOpeningCard = false)
    {
        _cards.Add(card);
        if (isOpeningCard && _openingHighAceIndex < 0 && card.Rank == CardRank.Ace) _openingHighAceIndex = _cards.Count - 1;
        RecalculateScore();
    }

    // 마지막으로 추가된 카드를 삭제하고 갱신된 점수를 반환합니다.
    public bool TryRemoveLastCard(out Card removedCard)
    {
        if (_cards.Count == 0)
        {
            removedCard = default;
            return false;
        }

        int _lastIndex = _cards.Count - 1;
        removedCard = _cards[_lastIndex];
        RemoveCardAt(_lastIndex);
        LastCardRemoved?.Invoke(removedCard, Score);
        return true;
    }

    // 현재 패를 비워 새 매치를 준비합니다.
    public void Clear()
    {
        _cards.Clear();
        _openingHighAceIndex = -1;
        Score = 0;
        IsSoft = false;
    }

    // 다른 패과 교환할 때 카드 목록과 초기 에이스의 11점 처리 상태를 함께 설정합니다.
    public void ReplaceCards(IReadOnlyList<Card> cards, int openingHighAceIndex)
    {
        _cards.Clear();
        if (cards != null)
        {
            for (int _index = 0; _index < cards.Count; _index++) _cards.Add(cards[_index]);
        }

        _openingHighAceIndex = openingHighAceIndex >= 0 && openingHighAceIndex < _cards.Count && _cards[openingHighAceIndex].Rank == CardRank.Ace
            ? openingHighAceIndex
            : -1;
        RecalculateScore();
    }

    // 패에서 무작위 카드 한 장을 제거합니다. 제거된 카드는 덱으로 반환하지 않습니다.
    public bool TryRemoveRandomCard(Random random, out Card removedCard)
    {
        if (_cards.Count == 0 || random == null)
        {
            removedCard = default;
            return false;
        }

        int _cardIndex = random.Next(_cards.Count);
        removedCard = _cards[_cardIndex];
        RemoveCardAt(_cardIndex);
        return true;
    }

    // Console 또는 UI용 카드 목록을 반환합니다.
    public string ToDisplayString() => string.Join(", ", _cards.Select(card => card.ToString()));

    // 초기 패에서 얻은 첫 에이스만 11점으로 계산하고, 이후 에이스는 항상 1점으로 계산합니다.
    private void RecalculateScore()
    {
        int _score = _cards.Sum(card => card.BaseValue);
        IsSoft = _openingHighAceIndex >= 0;
        if (_openingHighAceIndex >= 0) _score += 10;
        Score = _score;
    }

    // 지정한 인덱스의 카드를 제거하고 초기 에이스 인덱스와 점수를 함께 갱신합니다.
    private void RemoveCardAt(int cardIndex)
    {
        _cards.RemoveAt(cardIndex);
        if (_openingHighAceIndex == cardIndex) _openingHighAceIndex = -1;
        else if (_openingHighAceIndex > cardIndex) _openingHighAceIndex--;
        RecalculateScore();
    }
}
