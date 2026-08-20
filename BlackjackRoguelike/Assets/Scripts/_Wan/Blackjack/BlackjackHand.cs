using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>한 참가자가 가진 카드와 에이스 규칙이 적용된 현재 점수를 관리합니다.</summary>
public sealed class BlackjackHand
{
    private readonly List<Card> _cards = new();
    private readonly int _targetScore;
    private bool _hasOpeningHighAce;

    // 현재 패입니다.
    public IReadOnlyList<Card> Cards => _cards;
    // 현재 점수입니다.
    public int Score { get; private set; }
    // 소프트 핸드 여부입니다.
    public bool IsSoft { get; private set; }
    // 패에 에이스가 한 장 이상 있는지 나타냅니다.
    public bool HasAce => _cards.Any(card => card.Rank == CardRank.Ace);
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
        if (isOpeningCard && !_hasOpeningHighAce && card.Rank == CardRank.Ace) _hasOpeningHighAce = true;
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
        _cards.RemoveAt(_lastIndex);
        RecalculateScore();
        LastCardRemoved?.Invoke(removedCard, Score);
        return true;
    }

    // 현재 패를 비워 새 매치를 준비합니다.
    public void Clear()
    {
        _cards.Clear();
        _hasOpeningHighAce = false;
        Score = 0;
        IsSoft = false;
    }

    // Console 또는 UI용 카드 목록을 반환합니다.
    public string ToDisplayString() => string.Join(", ", _cards.Select(card => card.ToString()));

    // 초기 패에서 얻은 첫 에이스만 11점으로 계산하고, 이후 에이스는 항상 1점으로 계산합니다.
    private void RecalculateScore()
    {
        int _score = _cards.Sum(card => card.BaseValue);
        IsSoft = _hasOpeningHighAce;
        if (_hasOpeningHighAce) _score += 10;
        Score = _score;
    }
}
