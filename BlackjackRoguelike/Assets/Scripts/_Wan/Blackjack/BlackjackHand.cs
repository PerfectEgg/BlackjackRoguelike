using System.Collections.Generic;
using System.Linq;

/// <summary>한 참가자가 가진 카드와 에이스 규칙이 적용된 현재 점수를 관리합니다.</summary>
public sealed class BlackjackHand
{
    private readonly List<Card> _cards = new();
    private readonly int _targetScore;

    // 현재 패입니다.
    public IReadOnlyList<Card> Cards => _cards;
    // 현재 점수입니다.
    public int Score { get; private set; }
    // 소프트 핸드 여부입니다.
    public bool IsSoft { get; private set; }
    // 블랙잭 여부입니다.
    public bool IsBlackjack => _cards.Count == 2 && Score == 21;

    // 에이스 계산에 사용할 목표 점수를 지정합니다.
    public BlackjackHand(int targetScore = 21)
    {
        _targetScore = targetScore;
    }

    // 카드를 추가하고 점수를 다시 계산합니다.
    public void Add(Card card)
    {
        _cards.Add(card);
        RecalculateScore();
    }

    // 현재 패를 비워 새 매치를 준비합니다.
    public void Clear()
    {
        _cards.Clear();
        Score = 0;
        IsSoft = false;
    }

    // Console 또는 UI용 카드 목록을 반환합니다.
    public string ToDisplayString() => string.Join(", ", _cards.Select(card => card.ToString()));

    // 에이스를 1점으로 계산한 뒤 가능한 에이스만 11점으로 올립니다.
    private void RecalculateScore()
    {
        int _score = _cards.Sum(card => card.BaseValue);
        int _aceCount = _cards.Count(card => card.Rank == CardRank.Ace);
        IsSoft = false;
        while (_aceCount > 0 && _score + 10 <= _targetScore)
        {
            _score += 10;
            _aceCount--;
            IsSoft = true;
        }
        Score = _score;
    }
}
