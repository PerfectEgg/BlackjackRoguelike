/// <summary>블랙잭 매치에서 변경 가능한 기본 규칙을 모아 둔 설정 클래스입니다.</summary>
public sealed class MatchRules
{
    // 목표 점수입니다.
    public int TargetScore { get; }
    // 딜러 스탠드 점수입니다.
    public int DealerStandScore { get; }
    // 시작 카드 수입니다.
    public int StartingHandSize { get; }

    // 필요한 규칙만 변경해 매치 규칙을 생성합니다.
    public MatchRules(int targetScore = 21, int dealerStandScore = 17, int startingHandSize = 2)
    {
        TargetScore = targetScore;
        DealerStandScore = dealerStandScore;
        StartingHandSize = startingHandSize;
    }
}
