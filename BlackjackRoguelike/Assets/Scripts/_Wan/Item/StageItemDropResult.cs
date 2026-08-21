using System.Collections.Generic;

/// <summary>스테이지 처치 보상으로 생성된 아이템 후보와 선택 가능 횟수를 담습니다.</summary>
public class StageItemDropResult
{
    // 2개 중 1개를 선택할 패시브 후보입니다.
    public IReadOnlyList<ItemDefinition> PassiveCandidates { get; }
    // 3개 중 2개를 선택할 액티브 후보입니다.
    public IReadOnlyList<ItemDefinition> ActiveCandidates { get; }
    // 패시브 후보가 없으면 0개, 있으면 최대 1개를 선택합니다.
    public int PassivePickCount => System.Math.Min(1, PassiveCandidates.Count);
    // 액티브 후보 수가 부족할 수 있으므로 최대 2개까지만 선택합니다.
    public int ActivePickCount => System.Math.Min(2, ActiveCandidates.Count);

    // 드롭 결과를 생성합니다.
    public StageItemDropResult(List<ItemDefinition> passiveCandidates, List<ItemDefinition> activeCandidates)
    {
        PassiveCandidates = passiveCandidates;
        ActiveCandidates = activeCandidates;
    }
}
