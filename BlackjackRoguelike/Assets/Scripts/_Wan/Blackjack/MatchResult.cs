/// <summary>완료된 블랙잭 매치의 결과를 전투 시스템에 전달하는 값 객체입니다.</summary>
public readonly struct MatchResult
{
    // 매치의 승패입니다.
    public MatchOutcome Outcome { get; }
    // 매치 종료 시점 플레이어의 점수입니다.
    public int PlayerScore { get; }
    // 매치 종료 시점 딜러의 점수입니다.
    public int DealerScore { get; }
    // 플레이어가 두 장 블랙잭을 만들었는지 나타냅니다.
    public bool PlayerBlackjack { get; }
    // 딜러가 두 장 블랙잭을 만들었는지 나타냅니다.
    public bool DealerBlackjack { get; }

    // 두 패의 상태로 결과를 생성합니다.
    public MatchResult(MatchOutcome outcome, BlackjackHand playerHand, BlackjackHand dealerHand)
    {
        Outcome = outcome;
        PlayerScore = playerHand.Score;
        DealerScore = dealerHand.Score;
        PlayerBlackjack = playerHand.IsBlackjack;
        DealerBlackjack = dealerHand.IsBlackjack;
    }
}
