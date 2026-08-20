using UnityEngine;

/// <summary>1~10스테이지의 몬스터 정의 에셋을 순서대로 관리합니다.</summary>
[CreateAssetMenu(menuName = "Blackjack/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    // 이번 런에서 사용할 고정 스테이지 수입니다.
    public const int StageCount = 10;

    [Tooltip("인덱스 0부터 스테이지 1이며, 정확히 10개의 몬스터를 등록합니다.")]
    public MonsterDefinition[] StageMonsters = new MonsterDefinition[StageCount];

    // 스테이지 번호에 대응하는 몬스터 정의를 반환합니다. 범위를 벗어나면 null을 반환합니다.
    public MonsterDefinition GetMonster(int stageNumber)
    {
        if (stageNumber < 1 || stageNumber > StageMonsters.Length) return null;
        return StageMonsters[stageNumber - 1];
    }

    // Inspector에서 배열 길이가 항상 10칸으로 유지되도록 합니다.
    private void OnValidate()
    {
        if (StageMonsters == null || StageMonsters.Length != StageCount) System.Array.Resize(ref StageMonsters, StageCount);
    }
}
